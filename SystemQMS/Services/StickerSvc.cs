using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Hubs;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class StickerSvc : IStickerSvc
    {
        private MyDbContext _context;
        public StickerSvc (MyDbContext context)
        {
            _context = context;
        }


        private IHubContext<SessionHub> _hubContext;
        public StickerSvc(
            MyDbContext context,
            IHubContext<SessionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public string AddNew(Sticker sticker)
        {
            var service = _context.services.FirstOrDefault(x => x.id == sticker.serviceId);
            if(service == null)
            {
                return "Không tìm thấy dịch vụ";
            }

             var counter = _context.counters
            .FirstOrDefault(x => x.id == sticker.counterId);

            if (counter == null)
            {
                return "Không tìm thấy quầy dịch vụ";
            }
            if (!service.isActive)
                return "Dịch vụ hiện không hoạt động";
            var fomular = _context.numberFomulars.FirstOrDefault(x => x.serviceId == service.id);
            if(fomular== null)
            {
                return "Dịch vụ chưa có quy tắc số";
            }



            if (fomular.startNumber > fomular.endNumber)
                return "Dãy số không hợp lệ.";

            


            // tim so da cap thuoc day hien tai
            var tickerId = _context.stickers.Where(x => x.serviceId == service.id)
                .Select(x => x.sttId)
                .ToList();


            int nextNumber;

            if (fomular.sequenceMode == "Shared")
            {
                // lay tat ca ve da cap toan he thong

                var issuedNumbers = _context.stickers
                    .Select(x => x.sttId)
                    .ToList()

                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number) ? (int?)number : null;
                    }
                    )
                    .Where(number => number.HasValue
                && number.Value >= fomular.startNumber
                && number.Value <= fomular.endNumber
                )
                .Select(number => number!.Value)
                .ToList();


                // so lon nhat da cap tren toan he thng


                int maxNumber = issuedNumbers.Any()
                ? issuedNumbers.Max() : fomular.startNumber - 1;


                // tinh tien 1

                nextNumber = maxNumber + 1;
            }
            else
            {
                // che do doc lap
                // chi lay so da cap thuoc dich vu hien tai
                var issuedNumbers = _context.stickers
                    .Where(x => x.serviceId == service.id)
                    .Select(x => x.sttId)
                    .ToList()
                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number)
                        ? (int?)number : null;
                    })
                    .Where(number => number.HasValue)
                    .Select(number => number!.Value)
                    .ToList();
                nextNumber = issuedNumbers.Any()
                    ? issuedNumbers.Max() + 1 : fomular.startNumber;

            }

            // kiem tra gioi han 
            if (nextNumber > fomular.endNumber)
                return "Dãy số đã hết";

           

            //tao phieu
            string nextStt = fomular.firstName + nextNumber;
            string numberText = nextNumber.ToString("D5");

            string sttId;

            if (!string.IsNullOrEmpty(fomular.firstName))
            {
                sttId = fomular.firstName + numberText;
            }
            else if (!string.IsNullOrEmpty(fomular.lastName))
            {
                sttId = numberText + fomular.lastName;
            }
            else
            {
                sttId = numberText;
            }



            // kiem tra trung ma phieu

            if (_context.stickers.Any(x => x.sttId == nextStt))
                return "Số thứ tự đã tồn tại ";

            // tao phieu

            var newSticker = new Sticker
            {
                sttId = sttId,
                unitId = service.unitId,
                serviceId = service.id,
                counterId = counter.id,
                userId = sticker.userId,
                issuanceTime = DateTimeOffset.UtcNow,
                useTime = DateTimeOffset.UtcNow,
                status = "Waiting",
                stage = "Waiting"


            };

            // luu dl

            _context.stickers.Add(newSticker);
            _context.SaveChanges();

            return "Cấp số thành công: " + newSticker;

            
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
            sticker.useTime = DateTimeOffset.Now;

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
                    skippedAd = DateTimeOffset.Now
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

            if (sticker.status != "Waiting " && sticker.status != "Calling")
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