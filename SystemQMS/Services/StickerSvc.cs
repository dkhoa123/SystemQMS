using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.Sticker;

namespace SystemQMS.Services
{
    public class StickerSvc : IStickerSvc
    {
        private MyDbContext _context;
        //public StickerSvc (MyDbContext context)
        //{
        //    _context = context;
        //}


        private IHubContext<SessionHub> _hubContext;
        public StickerSvc(
            MyDbContext context,
            IHubContext<SessionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<string> AddNew(CreateStickerDto dto)
        {
            //  Kiểm tra dịch vụ
            var service = await _context.services
                .FirstOrDefaultAsync(x => x.id == dto.ServiceId);

            if (service == null)
                return "Không tìm thấy dịch vụ.";

            if (!service.isActive)
                return "Dịch vụ hiện không hoạt động.";

            // Kiểm tra quầy
            var counter = await _context.counters
                .FirstOrDefaultAsync(x => x.id == dto.CounterId);

            if (counter == null)
                return "Không tìm thấy quầy dịch vụ.";

            //  Kiểm tra nhân viên
            var user = await _context.users
                .FirstOrDefaultAsync(x => x.userId == dto.UserId);

            if (user == null)
                return "Không tìm thấy nhân viên.";

            //  Lấy công thức
            var fomular = await _context.numberFomulars
                .FirstOrDefaultAsync(x => x.serviceId == service.id);

            if (fomular == null)
                return "Dịch vụ chưa có quy tắc số.";

            if (fomular.startNumber < 0 ||
                fomular.endNumber < fomular.startNumber)
                return "Dãy số không hợp lệ.";

            //  Xác định thời gian Việt Nam
            var now = DateTimeOffset.UtcNow;
            var vietnamNow = now.ToOffset(TimeSpan.FromHours(7));

            var startOfDayVn = new DateTimeOffset(
                vietnamNow.Year,
                vietnamNow.Month,
                vietnamNow.Day,
                0, 0, 0,
                TimeSpan.FromHours(7));

            // Chuyển mốc thời gian sang UTC trước khi truy vấn PostgreSQL
            var startOfDay = startOfDayVn.ToUniversalTime();
            var endOfDay = startOfDay.AddDays(1);

            int nextNumber;

            //  Xử lý Shared
            if (fomular.sequenceMode == "Shared")
            {
                // Lấy tất cả dịch vụ đang sử dụng Shared
                var sharedServiceIds = await _context.numberFomulars
                    .Where(x => x.sequenceMode == "Shared")
                    .Select(x => x.serviceId)
                    .ToListAsync();

                // Lấy tất cả phiếu đã cấp trong ngày của nhóm Shared
                var issuedIds = await _context.stickers
                    .Where(x => sharedServiceIds.Contains(x.serviceId))
                    .Where(x => x.issuanceTime >= startOfDay &&
                                x.issuanceTime < endOfDay)
                    .Select(x => x.sttId)
                    .ToListAsync();

                var issuedNumbers = issuedIds
                    .Select(sttId =>
                    {
                        string digits = new string(
                            sttId.Where(char.IsDigit).ToArray()
                        );

                        return int.TryParse(digits, out int number)
                            ? (int?)number
                            : null;
                    })
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

                // Tính số tiếp theo
                nextNumber = issuedNumbers.Any()
                    ? issuedNumbers.Max() + 1
                    : fomular.startNumber;

                // Kiểm tra giới hạn tổng
                if (nextNumber > fomular.endNumber)
                    throw new Exception("Dãy số đã hết.");

                // Kiểm tra giới hạn cấp số trong ngày
                if (fomular.isDailyLimitEnabled)
                {
                    if (!fomular.dailyEndNumber.HasValue)
                        throw new Exception("Chưa cấu hình giới hạn số cuối ngày.");

                    if (nextNumber > fomular.dailyEndNumber.Value)
                        throw new Exception("Đã hết số cấp trong ngày.");
                }
            }
            //  Xử lý Independent
            else if (fomular.sequenceMode == "Independent")
            {
                var issuedIds = await _context.stickers
                    .Where(x => x.serviceId == service.id)
                    .Where(x => x.issuanceTime >= startOfDay &&
                                x.issuanceTime < endOfDay)
                    .Select(x => x.sttId)
                    .ToListAsync();

                var issuedNumbers = issuedIds
                    .Select(sttId =>
                    {
                        string digits = new string(
                            sttId.Where(char.IsDigit).ToArray());

                        return int.TryParse(digits, out int number)
                            ? (int?)number
                            : null;
                    })
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

                nextNumber = issuedNumbers.Any()
                    ? Math.Max(
                        fomular.startNumber,
                        issuedNumbers.Max() + 1)
                    : fomular.startNumber;

                if (nextNumber > fomular.endNumber)
                    return "Dãy số đã hết.";
            }
            else
            {
                return "Chế độ cấp số không hợp lệ.";
            }

            //  Tạo mã phiếu
            string numberText = nextNumber.ToString("D5");

            string sttId;

            if (fomular.numberType == "Prefix")
            {
                sttId = fomular.firstName + numberText;
            }
            else if (fomular.numberType == "Suffix")
            {
                sttId = numberText + fomular.lastName;
            }
            else
            {
                return "Loại định dạng không hợp lệ.";
            }

            //  Kiểm tra mã phiếu trùng
            bool exists = await _context.stickers
                .AnyAsync(x => x.sttId == sttId);

            if (exists)
                return "Số thứ tự đã tồn tại.";

            //  Tạo phiếu
            var newSticker = new Sticker
            {
                sttId = sttId,
                unitId = service.unitId,
                serviceId = service.id,
                counterId = counter.id,
                userId = dto.UserId,
                issuanceTime = now,
                useTime = now,
                status = "Waiting",
                stage = "Waiting"
            };

            _context.stickers.Add(newSticker);

            await _context.SaveChangesAsync();

            // Thông báo đến các màn hình quầy
            await _hubContext.Clients
           .Group($"Counter_{newSticker.counterId}")
           .SendAsync("NumberCreated", new
     {
                    sttId = newSticker.sttId,
                    serviceId = newSticker.serviceId,
                    counterId = newSticker.counterId,
                    status = newSticker.status,
                    issuanceTime = newSticker.issuanceTime
                });

            return "Cấp số thành công: " + newSticker.sttId;
        }




        public async Task<List<StickerListDto>> GetStickerList()
        {
            var data = await _context.stickers
                .AsNoTracking()
                .Include(x => x.Users)
                .Include(x => x.Service)
                .Include(x => x.Counter)
                .OrderByDescending(x => x.issuanceTime)
                .Select(x => new StickerListDto
                {
                    SttId = x.sttId,
                    Status = x.status,

                    Handler = x.Users != null
                        ? x.Users.email
                        : "",

                    IssuanceTime = x.issuanceTime,

                    UseTime = x.status == "Completed" ||
                              x.status == "Absent"
                        ? x.useTime
                        : null,

                    ServiceName = x.Service != null
                        ? x.Service.serviceName
                        : "",

                    UserName = x.Users != null
                        ? x.Users.email
                        : "",

                    CounterName = x.Counter != null
                        ? x.Counter.counterName
                        : "",

                    CancelReason = x.cancelReason
                })
                .ToListAsync();

            return data;
        }



        public async Task<List<Sticker>> CallMultiple(int counterId, int quantity)
        {
            if (quantity < 1 || quantity > 6)
                throw new ArgumentException("Số lượng gọi phải từ 1 đến 6");
            var counter = await _context.counters.FirstOrDefaultAsync(x => x.id == counterId);
            if (counter == null)
                throw new Exception("Quầy không tồn tại");

            // lay phiieu cho
            var stickers = await _context.stickers
                .Where(x => x.status == "Waiting")
                .OrderBy(x => x.issuanceTime)
                .Take(quantity)
                .ToListAsync();

            // kiem tra hang cho
            if (stickers.Count == 0)
                return stickers;

            // cap nhat trang thai phieu

            foreach(var sticker in stickers)
            {
                sticker.status = "Calling";
                sticker.counterId = counterId;
                sticker.useTime = DateTimeOffset.Now;
                 
            }

            await _context.SaveChangesAsync();

            return stickers;
        }

        public async Task<Sticker?> CallNext(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(x => x.id == counterId);
            if (counter == null)
            {
                throw new Exception(" Quầy không tồn tại");
            }

            // tim phieu
            var sticker = await _context.stickers
                .Where(x => x.status == "Waiting")
                .OrderBy(x => x.issuanceTime)
                .FirstOrDefaultAsync();
           if(sticker == null)
            {
                return null;
            }


            //cap nhat phieu

            sticker.status = "Calling";
            sticker.counterId = counterId;
            sticker.useTime = DateTimeOffset.Now;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                 "NumbersCalled",
                 new

                 {
                     counterId,
                     sticker,
                     calledAt = DateTimeOffset.Now
                 });
            return sticker;
        }

        public byte[] ExportExcel()
        {
            var data = _context.stickers.ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Stickers");

            sheet.Cell(1, 1).Value = "STT";
            sheet.Cell(1, 2).Value = "Trạng thái";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cell(row, 1).Value = item.sttId;
                sheet.Cell(row, 2).Value = item.status;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        public async Task<int> ResetNumbers()
        {
            // Lấy ngày hiện tại theo giờ Việt Nam
            var vietnamTime = DateTimeOffset.UtcNow.ToOffset(
                TimeSpan.FromHours(7));

            var startOfDay = new DateTimeOffset(
                vietnamTime.Year,
                vietnamTime.Month,
                vietnamTime.Day,
                0, 0, 0,
                TimeSpan.FromHours(7));

            var endOfDay = startOfDay.AddDays(1);

            // Chuyển về UTC để truy vấn PostgreSQL
            var startUtc = startOfDay.ToUniversalTime();
            var endUtc = endOfDay.ToUniversalTime();

            // Lấy tất cả STT được cấp trong ngày
            var stickers = await _context.stickers
                .Where(x =>
                    x.issuanceTime >= startUtc &&
                    x.issuanceTime < endUtc &&
                    x.status != "Cancelled")
                .ToListAsync();

            if (!stickers.Any())
                return 0;

            // Chuyển tất cả STT sang trạng thái Đã hủy
            foreach (var sticker in stickers)
            {
                sticker.status = "Cancelled";
                sticker.cancelReason = "Reset quy tắc cấp số";
            }

            await _context.SaveChangesAsync();

            // Lấy danh sách mã STT vừa hủy
            var sttIds = stickers
                .Select(x => x.sttId)
                .ToList();

            // Thông báo đến tất cả màn hình qua SignalR
            await _hubContext.Clients.All.SendAsync(
                "NumbersReset",
                new
                {
                    sttIds,
                    reason = "Reset quy tắc cấp số",
                    resetAt = DateTimeOffset.UtcNow
                });

            return stickers.Count;
        }


        public async Task<Sticker?> CallAgain(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(x => x.id == counterId);
            if (counter == null)
                throw new Exception("Quầy không tồn tại");

            // tim stt dang dc goi tai quay
            var sticker = await _context.stickers
                .Where(x => x.counterId == counterId && x.status == "Calling")
                .OrderByDescending(x => x.useTime)
                .FirstOrDefaultAsync();

            if (sticker == null)
                return null;

            //cap naht thoi diem goi
            sticker.useTime = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            // phat su kien signalR
            await _hubContext.Clients.All.SendAsync(
                "NumbersCalled",
                new
                {
                    counterId,
                    sticker = new List<Sticker> { sticker },
                    calledAt = sticker.useTime
                });


            return sticker;
        }

        public async Task<Sticker?> Skip(int counterId)
        {
            var counter = await _context.counters.FirstOrDefaultAsync(x => x.id == counterId);
            var sticker = await _context.stickers.Where(x => x.counterId == counterId && x.status == "Calling")
                .OrderByDescending(x => x.useTime)
                .FirstOrDefaultAsync();
            if (sticker == null)
                return null;
            // chuyen trang thai

            sticker.status = "Absent";

            //luu

            await _context.SaveChangesAsync();
            // phat su kien

            await _hubContext.Clients.All.SendAsync(
                "NumbersCalled",
                new
                {
                    counterId,
                    sticker,
                    skippedAd = DateTimeOffset.UtcNow
                });

            return sticker;
        }

        public async Task<Sticker?> TransferCounter(string sttId, int targetCounterId)
        {
            var sticker = await _context.stickers
                .FirstOrDefaultAsync(x => x.sttId == sttId);
            if (sticker == null)
                return null;

            var targetCounter = await _context.counters.FirstOrDefaultAsync(x => x.id == targetCounterId);
            if (targetCounter == null)
                throw new Exception("Quầy đích không tồn tại.");

            if (targetCounterId == sticker.counterId)
                throw new Exception("STT đang ở quầy này.");

            if (sticker.status != "Waiting" && sticker.status != "Calling")
                throw new Exception("Chỉ được chuyển STT đang chờ hoặc đang xử lý");

            if(sticker.status== "Waiting")
            {
                sticker.status = "Calling";
                sticker.useTime = DateTimeOffset.Now;
            }
            // chuyen quay

            int sourceCounterId = sticker.counterId;
            sticker.counterId = targetCounterId;

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync(
                "NumberTransferred",
                new
                {
                    sttId = sticker.sttId,
                    sourceCounterId,
                    targetCounterId,
                    status = sticker.status
                });

            return sticker;
            
        }

        public async Task<List<Sticker>> TransferSelected(List<string> sttIds, int targetCounterId)
        {
            var targetCouter = await _context.counters.FirstOrDefaultAsync(x => x.id == targetCounterId);
            if (targetCounterId == null)
                throw new Exception("Quầy đích không tồn tại");

            // cac stt duoc chon
            var stickers = await _context.stickers
                .Where(x => sttIds.Contains(x.sttId))
                .ToListAsync();

            if (!stickers.Any())
                throw new Exception("Không tìm thấy stt được chọn");

            // kiem tra va chuyen  tung stt
            foreach (var sticker in stickers)
            {
                if (sticker.status != "Waiting" &&
                    sticker.status != "Calling")
                {
                    continue;
                }

                if (sticker.status == "Waiting")
                {
                    sticker.status = "Calling";
                    sticker.useTime = DateTimeOffset.Now;
                }
                if (sticker.counterId == targetCounterId)
                    continue;
                sticker.counterId = targetCounterId;


            }
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync(
                "NumbersTransferred",
                    new
                    {
                        targetCounterId,
                        stickers
                    });

            return stickers;

        }
    }
}