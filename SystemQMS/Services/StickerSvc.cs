using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.PageResultDto;
using SystemQMS.ViewModels.TicketDto;

namespace SystemQMS.Services
{
    public class StickerSvc : IStickerSvc
    {
        private MyDbContext _context;
        public StickerSvc (MyDbContext context)
        {
            _context = context;
        }
        private static TimeZoneInfo GetVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "SE Asia Standard Time"
                );
            }
            catch
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Asia/Ho_Chi_Minh"
                );
            }
        }

        private static (DateTimeOffset Start, DateTimeOffset End)
            GetVietnamDayRange()
        {
            var vnTimeZone = GetVietnamTimeZone();

            var now = TimeZoneInfo.ConvertTime(
                DateTimeOffset.UtcNow,
                vnTimeZone
            );

            var localStart = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                0, 0, 0,
                DateTimeKind.Unspecified
            );

            var localEnd = localStart.AddDays(1);

            var start = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localStart,
                    vnTimeZone
                )
            );

            var end = new DateTimeOffset(
                TimeZoneInfo.ConvertTimeToUtc(
                    localEnd,
                    vnTimeZone
                )
            );

            return (start, end);
        }

        private static int ExtractNumber(string ticketNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                return 0;

            var digits = new string(
                ticketNumber
                    .Reverse()
                    .TakeWhile(char.IsDigit)
                    .Reverse()
                    .ToArray()
            );

            return int.TryParse(digits, out var number)
                ? number
                : 0;
        }

        private async Task<int> GetCurrentNumberAsync(
    int unitId,
    int serviceId,
    bool resetDaily)
        {
            var query = _context.Tickets
                .Where(x =>
                    x.UnitId == unitId &&
                    x.ServiceId == serviceId
                );

            if (resetDaily)
            {
                var (start, end) = GetVietnamDayRange();

                query = query.Where(x =>
                    x.IssuanceTime >= start &&
                    x.IssuanceTime < end
                );
            }

            var ticketNumbers = await query
                .Select(x => x.TicketNumber)
                .ToListAsync();

            return ticketNumbers
                .Select(ExtractNumber)
                .DefaultIfEmpty(0)
                .Max();
        }
        public string AddNew(Tickets sticker)
        {
            var service = _context.Services.FirstOrDefault(x => x.Id == sticker.ServiceId);
            if(service == null)
            {
                return "Không tìm thấy dịch vụ";
            }

             var counter = _context.Counters
            .FirstOrDefault(x => x.Id == sticker.CounterId);

            if (counter == null)
            {
                return "Không tìm thấy quầy dịch vụ";
            }
            if (!service.IsActive)
                return "Dịch vụ hiện không hoạt động";
            var fomular = _context.NumberFomulars.FirstOrDefault(x => x.ServiceId == service.Id);
            if(fomular== null)
            {
                return "Dịch vụ chưa có quy tắc số";
            }



            if (fomular.StartNumber > fomular.EndNumber)
                return "Dãy số không hợp lệ.";

            


            // tim so da cap thuoc day hien tai
            var tickerId = _context.Tickets.Where(x => x.ServiceId == service.Id)
                .Select(x => x.SttId)
                .ToList();


            int nextNumber;

            if (fomular.SequenceMode == "Shared")
            {
                // lay tat ca ve da cap toan he thong

                var issuedNumbers = _context.Tickets
                    .Select(x => x.SttId)
                    .ToList()

                    .Select(sttId =>
                    {
                        string digits = new string(sttId.Where(char.IsDigit).ToArray());
                        return int.TryParse(digits, out int number) ? (int?)number : null;
                    }
                    )
                    .Where(number => number.HasValue
                && number.Value >= fomular.StartNumber
                && number.Value <= fomular.EndNumber
                )
                .Select(number => number!.Value)
                .ToList();


                // so lon nhat da cap tren toan he thng


                int maxNumber = issuedNumbers.Any()
                ? issuedNumbers.Max() : fomular.StartNumber - 1;


                // tinh tien 1

                nextNumber = maxNumber + 1;
            }
            else
            {
                // che do doc lap
                // chi lay so da cap thuoc dich vu hien tai
                var issuedNumbers = _context.Tickets
                    .Where(x => x.ServiceId == service.Id)
                    .Select(x => x.SttId)
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
                    ? issuedNumbers.Max() + 1 : fomular.StartNumber;

            }

            // kiem tra gioi han 
            if (nextNumber > fomular.EndNumber)
                return "Dãy số đã hết";

           

            //tao phieu
            string nextStt = fomular.FirstName + nextNumber;
            string numberText = nextNumber.ToString("D5");

            string sttId;

            if (!string.IsNullOrEmpty(fomular.FirstName))
            {
                sttId = fomular.FirstName + numberText;
            }
            else if (!string.IsNullOrEmpty(fomular.LastName))
            {
                sttId = numberText + fomular.LastName;
            }
            else
            {
                sttId = numberText;
            }



            // kiem tra trung ma phieu

            if (_context.Tickets.Any(x => x.SttId == nextStt))
                return "Số thứ tự đã tồn tại ";

            // tao phieu

            var newSticker = new Tickets
            {
                SttId = sttId,
                UnitId = service.UnitId,
                ServiceId = service.Id,
                CounterId = counter.Id,
                UserId = sticker.UserId,
                IssuanceTime = DateTimeOffset.UtcNow,
                UseTime = DateTimeOffset.UtcNow,
                Status = "Waiting",
                Stage = "Waiting"


            };

            // luu dl

            _context.Tickets.Add(newSticker);
            _context.SaveChanges();

            return "Cấp số thành công: " + newSticker;

            
        }

        public byte[] ExportExcel()
        {
            var data = _context.Tickets.ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Stickers");

            sheet.Cell(1, 1).Value = "STT";
            sheet.Cell(1, 2).Value = "Trạng thái";

            int row = 2;
            foreach (var item in data)
            {
                sheet.Cell(row, 1).Value = item.SttId;
                sheet.Cell(row, 2).Value = item.Status;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public TicketStatisticsDto GetStatistics()
        {
            var ticketStatistics = new TicketStatisticsDto
            {
                totalIssued = _context.Tickets.Count(),
                waiting = _context.Tickets.Count(t => t.Status == "Waiting"),
                isUsed = _context.Tickets.Count(t => t.Status == "Used"),
                skipped = _context.Tickets.Count(t => t.Status == "Skipped")
            };
            return ticketStatistics;
        }

        public PageResultDto<GetTicketDto> GetTickets(FilterTicketDto filter)
        {
            var query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.Service)
                .Include(t => t.Users)
                .Include(t => t.Counter)
                .AsQueryable();
            if (!string.IsNullOrEmpty(filter.keyword))
            {
                query = query.Where(t => t.SttId.Contains(filter.keyword));
            }
            if(!string.IsNullOrEmpty(filter.serviceName))
            {
                query = query.Where(t => t.Service.ServiceName.Contains(filter.serviceName));
            }
            if(!string.IsNullOrEmpty(filter.status))
            {
                query = query.Where(t => t.Status == filter.status);
            }
            if(filter.startDate.HasValue)
            {
                query = query.Where(t => t.IssuanceTime >= filter.startDate);
            }
            if(filter.endDate.HasValue)
            {
                query = query.Where(t => t.IssuanceTime <= filter.endDate);
            }

            var tickets = query.Skip((filter.page - 1) * filter.pageSize).Take(filter.pageSize).ToList();

            var ticketDtos = tickets.Select(t => new GetTicketDto
            {
                id = t.SttId.ToString(),
                ticketNumber = t.TicketNumber,
                serviceName = t.Service.ServiceName,
                issuedTime = t.IssuanceTime.ToString(),
                usedTime = t.UseTime.ToString(),
                status = t.Status,
                staffName = t.Users.Name,
                counterName = t.Counter.CounterName
            }).ToList();

            return new PageResultDto<GetTicketDto>
            {
                items = ticketDtos,
                total = query.Count(),
                page = filter.page,
                pageSize = filter.pageSize
            };
        }
        private static bool IsDailySequence(string? sequenceMode)
        {
            return string.Equals(
                sequenceMode,
                "Daily",
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                sequenceMode,
                "Ngay",
                StringComparison.OrdinalIgnoreCase
            )
            || string.Equals(
                sequenceMode,
                "Hằng ngày",
                StringComparison.OrdinalIgnoreCase
            );
        }
        public async Task<List<TicketRuleDto>> GetRulesAsync(
    int unitId)
        {
            var formulas = await _context.NumberFomulars
                .AsNoTracking()
                .Include(x => x.Service)
                .Where(x => x.Service.UnitId == unitId)
                .ToListAsync();

            var result = new List<TicketRuleDto>();

            foreach (var formula in formulas)
            {
                // SequenceMode cần đối chiếu với giá trị thực tế trong DB.
                bool resetDaily = IsDailySequence(
                    formula.SequenceMode
                );

                var currentNumber = await GetCurrentNumberAsync(
                    unitId,
                    formula.ServiceId,
                    resetDaily
                );

                result.Add(new TicketRuleDto
                {
                    Id = formula.Id,

                    ServiceId = formula.ServiceId,

                    ServiceName = formula.Service.ServiceName,

                    Prefix = formula.FirstName + formula.LastName,

                    StartNumber = formula.StartNumber,

                    EndNumber = formula.EndNumber,

                    CurrentNumber = currentNumber,

                    ResetDaily = resetDaily
                });
            }

            return result;
        }
        public async Task<bool> UpdateRangeAsync(
    int unitId,
    UpdateTicketRangeDto request)
        {
            if (request.StartNumber < 1)
                throw new ArgumentException(
                    "Số bắt đầu phải lớn hơn 0."
                );

            if (request.EndNumber < request.StartNumber)
                throw new ArgumentException(
                    "Số kết thúc phải lớn hơn hoặc bằng số bắt đầu."
                );

            var formulas = await _context.NumberFomulars
                .Include(x => x.Service)
                .Where(x => x.Service.UnitId == unitId)
                .ToListAsync();

            if (!formulas.Any())
                return false;

            foreach (var formula in formulas)
            {
                formula.StartNumber = request.StartNumber;
                formula.EndNumber = request.EndNumber;
            }

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<TicketResponseDto> IssueTicketAsync(
    int unitId,
    int userId,
    IssueTicketRequest request)
        {
            if (request.ServiceId <= 0)
                throw new ArgumentException("Dịch vụ không hợp lệ.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra tài khoản đăng nhập
                var user = await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.UnitId == unitId &&
                        x.IsActive
                    );

                if (user == null)
                    throw new Exception(
                        "Không tìm thấy tài khoản hoặc tài khoản đã bị khóa."
                    );

                // 2. Kiểm tra dịch vụ
                var service = await _context.Services
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ServiceId &&
                        x.UnitId == unitId &&
                        x.IsActive
                    );

                if (service == null)
                    throw new Exception(
                        "Dịch vụ không tồn tại hoặc đã ngừng hoạt động."
                    );

                // 3. Lấy quy tắc cấp số
                var formula = await _context.NumberFomulars
                    .FirstOrDefaultAsync(x =>
                        x.ServiceId == request.ServiceId
                    );

                if (formula == null)
                    throw new Exception(
                        "Dịch vụ chưa được thiết lập quy tắc cấp số."
                    );

                // 4. Khóa theo UnitId + ServiceId.
                // PostgreSQL sẽ giữ khóa đến khi transaction kết thúc.
                long lockKey =
                    ((long)unitId << 32) | (uint)request.ServiceId;

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock({lockKey})"
                );

                // 5. Kiểm tra lại quy tắc sau khi lấy khóa
                formula = await _context.NumberFomulars
                    .FirstAsync(x =>
                        x.ServiceId == request.ServiceId
                    );

                bool resetDaily = IsDailySequence(
                    formula.SequenceMode
                );

                // 6. Lấy số hiện tại
                int currentNumber = await GetCurrentNumberAsync(
                    unitId,
                    request.ServiceId,
                    resetDaily
                );

                // 7. Xác định số kết thúc
                int endNumber = formula.EndNumber;

                if (resetDaily && formula.IsDailyLimitEnabled)
                {
                    endNumber = formula.DailyEndNumber
                        ?? formula.EndNumber;
                }

                // 8. Sinh số tiếp theo
                int nextNumber = currentNumber == 0
                    ? formula.StartNumber
                    : currentNumber + 1;

                if (nextNumber < formula.StartNumber)
                    nextNumber = formula.StartNumber;

                if (nextNumber > endNumber)
                    throw new Exception(
                        "Đã hết số thứ tự trong dải số đã thiết lập."
                    );

                // 9. Tạo mã số
                string prefix =
                    formula.FirstName + formula.LastName;

                string ticketNumber =
                    prefix + nextNumber.ToString("D4");

                // 10. Kiểm tra CounterId
                var counter = await _context.Counters
                    .FirstOrDefaultAsync(x =>
                        x.Id == user.CounterId &&
                        x.UnitId == unitId
                    );

                if (counter == null)
                    throw new Exception(
                        "Tài khoản chưa được gán quầy hợp lệ."
                    );

                // 11. Thời gian cấp số
                var now = DateTimeOffset.UtcNow;

                // 12. Tạo vé
                var ticket = new Tickets
                {
                    SttId = Guid.NewGuid().ToString(),

                    UnitId = unitId,

                    ServiceId = service.Id,

                    CounterId = counter.Id,

                    UserId = user.UserId,

                    TicketNumber = ticketNumber,

                    IssuanceTime = now,

                    CallTime = null,

                    UseTime = now,

                    RecallCount = 0,

                    IsPriority = false,

                    Stage = "WAITING_ASSIGN",

                    Status = "Waiting"
                };

                _context.Tickets.Add(ticket);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 13. Trả dữ liệu
                return new TicketResponseDto
                {
                    Id = ticket.SttId,

                    TicketNumber = ticket.TicketNumber,

                    ServiceName = service.ServiceName,

                    IssueTime = ticket.IssuanceTime,

                    Status = ticket.Status
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}