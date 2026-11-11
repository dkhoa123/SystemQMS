
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;

using SystemQMS.ViewModels.NumberFomular;

namespace SystemQMS.Services
{
    public class NumberFomularSvc : INumberFomularSvc
    {
        private readonly MyDbContext _context;

        public NumberFomularSvc(MyDbContext context)
        {
            _context = context;
        }

        // TKT-04: Kiểm tra khoảng số Independent có bị trùng không
        private bool IsOverlap(NumberFomular formula)
        {
            return _context.numberFomulars.Any(x =>
                x.sequenceMode == "Independent" &&
                x.serviceId != formula.serviceId &&
                formula.startNumber <= x.endNumber &&
                formula.endNumber >= x.startNumber
            );
        }

        // TKT-04: Kiểm tra dữ liệu quy tắc
        private string? ValidateFormula(CreateNumberFomularDto dto)
        {
            if (dto.SequenceMode != "Shared" &&
                dto.SequenceMode != "Independent")
                return "Chế độ cấp số không hợp lệ.";

            if (dto.FormatSource != "ServiceCode" &&
                dto.FormatSource != "Custom")
                return "Nguồn định dạng không hợp lệ.";

            if (dto.NumberType != "Prefix" &&
                dto.NumberType != "Suffix")
                return "Loại định dạng phải là Prefix hoặc Suffix.";

            string prefix = dto.FirstName?.Trim() ?? "";
            string suffix = dto.LastName?.Trim() ?? "";

            if (prefix.Length > 3 || suffix.Length > 3)
                return "Tiền tố và hậu tố tối đa 3 ký tự.";

            if (prefix.Contains(' ') || suffix.Contains(' '))
                return "Tiền tố và hậu tố không được chứa khoảng trắng.";

            if (prefix != "" && suffix != "")
                return "Chỉ được sử dụng tiền tố hoặc hậu tố.";

            if (dto.FormatSource == "ServiceCode" &&
                (prefix != "" || suffix != ""))
                return "Không cần nhập tiền tố hoặc hậu tố khi chọn mã dịch vụ.";

            if (dto.FormatSource == "Custom" &&
                prefix == "" && suffix == "")
                return "Vui lòng nhập tiền tố hoặc hậu tố.";

            if (dto.SequenceMode == "Independent")
            {
                if (dto.StartNumber < 1 ||
                    dto.EndNumber > 99999 ||
                    dto.StartNumber > dto.EndNumber)
                    return "Khoảng số bắt đầu và kết thúc không hợp lệ.";
            }

            if (dto.SequenceMode == "Shared" &&
                dto.IsDailyLimitEnabled)
            {
                if (!dto.DailyEndNumber.HasValue ||
                    dto.DailyEndNumber.Value < 1 ||
                    dto.DailyEndNumber.Value > 99999)
                    return "Số cuối ngày phải từ 1 đến 99999.";
            }

            return null;
        }

        // TKT-04: Tạo quy tắc cấp số
        public string AddNew(CreateNumberFomularDto dto)
        {
            if (dto == null)
                return "Dữ liệu không hợp lệ.";

            var service = _context.services
                .FirstOrDefault(x => x.id == dto.ServiceId);

            if (service == null)
                return "Không tìm thấy dịch vụ.";

            bool exists = _context.numberFomulars
                .Any(x => x.serviceId == dto.ServiceId);

            if (exists)
                return "Dịch vụ này đã có quy tắc cấp số.";

            string? error = ValidateFormula(dto);

            if (error != null)
                return error;

            string prefix = dto.FirstName?.Trim() ?? "";
            string suffix = dto.LastName?.Trim() ?? "";

            // Nếu lấy mã dịch vụ thì tự động lấy 3 ký tự đầu
            if (dto.FormatSource == "ServiceCode")
            {
                string serviceCode = new string(
                    service.serviceId
                        .Where(c => !char.IsWhiteSpace(c))
                        .ToArray()
                );

                if (serviceCode.Length == 0)
                    return "Mã dịch vụ không hợp lệ.";

                prefix = serviceCode.Length >= 3
                    ? serviceCode.Substring(0, 3)
                    : serviceCode;

                suffix = "";
            }

            var formula = new NumberFomular
            {
                serviceId = dto.ServiceId,
                numberType = dto.NumberType,
                firstName = prefix,
                lastName = suffix,

                startNumber = dto.SequenceMode == "Shared"
                    ? 1 : dto.StartNumber,

                endNumber = dto.SequenceMode == "Shared"
                    ? 99999 : dto.EndNumber,

                isDailyLimitEnabled = dto.SequenceMode == "Shared"
                    && dto.IsDailyLimitEnabled,

                dailyEndNumber = dto.SequenceMode == "Shared"
                    && dto.IsDailyLimitEnabled
                    ? dto.DailyEndNumber : null,

                limitDay = dto.LimitDay.HasValue
                  ? dto.LimitDay.Value.ToUniversalTime()
                         : DateTimeOffset.UtcNow,
                
                sequenceMode = dto.SequenceMode,
                formatSource = dto.FormatSource
            };

            if (formula.sequenceMode == "Independent" &&
                IsOverlap(formula))
                return "Khoảng số bị trùng với dịch vụ khác.";

            try
            {
                _context.numberFomulars.Add(formula);
                _context.SaveChanges();

                return "Tạo quy tắc cấp số thành công.";
            }
            catch (DbUpdateException)
            {
                return "Không thể lưu quy tắc cấp số vào database.";
            }
        }

        // Các phương thức dưới đây sẽ triển khai ở phần tiếp theo



        public async Task<string> Update(int id, UpdateNumberFomularDto dto)
        {
            var fomular = await _context.numberFomulars
                .FirstOrDefaultAsync(x => x.id == id);

            if (fomular == null)
                return "Không tìm thấy công thức.";

            bool isSwitchToShared =   fomular.sequenceMode == "Independent"  && dto.SequenceMode == "Shared";

                // Kiểm tra khoảng số
            if (dto.StartNumber < 0 || dto.EndNumber < dto.StartNumber)
                return "Khoảng số không hợp lệ.";

            if (dto.NumberType != "Prefix" && dto.NumberType != "Suffix")
                return "NumberType chỉ được là Prefix hoặc Suffix.";

            if (dto.SequenceMode != "Shared" &&
                dto.SequenceMode != "Independent" &&
                dto.SequenceMode != "Dependent")
                return "SequenceMode không hợp lệ.";

            if (dto.FormatSource != "ServiceCode" &&
                dto.FormatSource != "Custom")
                return "FormatSource không hợp lệ.";

            // Kiểm tra cấu hình Custom
            if (dto.FormatSource == "Custom")
            {
                string prefix = dto.FirstName ?? "";
                string suffix = dto.LastName ?? "";

                if (prefix.Contains(" ") || suffix.Contains(" "))
                    return "Prefix và Suffix không được chứa khoảng trắng.";

                if (prefix.Length > 3 || suffix.Length > 3)
                    return "Prefix và Suffix tối đa 3 ký tự.";

                if (dto.NumberType == "Prefix" && string.IsNullOrWhiteSpace(prefix))
                    return "Vui lòng nhập Prefix.";

                if (dto.NumberType == "Suffix" && string.IsNullOrWhiteSpace(suffix))
                    return "Vui lòng nhập Suffix.";
            }

            // Kiểm tra giới hạn số hằng ngày
            if (dto.IsDailyLimitEnabled)
            {
                if (!dto.DailyEndNumber.HasValue)
                    return "Vui lòng nhập số kết thúc giới hạn ngày.";

                if (dto.DailyEndNumber < dto.StartNumber ||
                    dto.DailyEndNumber > dto.EndNumber)
                    return "Giới hạn số ngày phải nằm trong khoảng số đã cấu hình.";
            }

            // Kiểm tra số đã cấp của dịch vụ khi sửa Independent
            if (dto.SequenceMode == "Independent")
            {
                var issuedIds = await _context.stickers
                    .Where(x => x.serviceId == fomular.serviceId)
                    
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

                if (issuedNumbers.Any())
                {
                    int maxIssuedNumber = issuedNumbers.Max();
                    int minIssuedNumber = issuedNumbers.Min();

                    // Không được thay đổi khoảng khiến số đã cấp nằm ngoài dãy mới
                    if (dto.StartNumber > minIssuedNumber)
                        return "Không thể thay đổi số bắt đầu vì đã có STT được cấp.";

                    if (dto.EndNumber <= maxIssuedNumber)
                        return "Dãy số thấp hơn số hiện tại, vui lòng nhập lớn hơn số hiện tại.";
                }
            }


            // Kiểm tra trùng khoảng số khi Independent
            if (dto.SequenceMode == "Independent")
            {
                bool isOverlap = await _context.numberFomulars.AnyAsync(x =>
                    x.id != id &&
                    x.sequenceMode == "Independent" &&
                    dto.StartNumber <= x.endNumber &&
                    dto.EndNumber >= x.startNumber);

                if (isOverlap)
                    return "Khoảng số bị trùng với công thức Independent khác.";
            }



            // Chuyển từ Independent sang Shared
            if (isSwitchToShared)
            {
                // Lấy danh sách dịch vụ đang Independent,
                // bao gồm cả công thức hiện tại trước khi đổi chế độ.
                var independentServiceIds = await _context.numberFomulars
                    .Where(x => x.sequenceMode == "Independent")
                    .Select(x => x.serviceId)
                    .ToListAsync();

                var issuedIds = await _context.stickers
                    .Where(x => independentServiceIds.Contains(x.serviceId))
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

                // Tìm số tiếp theo
                fomular.sharedNextNumber = issuedNumbers.Any()
                    ? issuedNumbers.Max() + 1
                    : dto.StartNumber;
            }

            // Chuyển từ Shared sang Independent
            if (fomular.sequenceMode == "Shared" &&
                dto.SequenceMode == "Independent")
            {
                fomular.sharedNextNumber = null;
            }


            // Cập nhật dữ liệu
            fomular.numberType = dto.NumberType;
            fomular.firstName = dto.FirstName ?? "";
            fomular.lastName = dto.LastName ?? "";
            fomular.startNumber = dto.StartNumber;
            fomular.endNumber = dto.EndNumber;
            fomular.isDailyLimitEnabled = dto.IsDailyLimitEnabled;
            fomular.dailyEndNumber = dto.IsDailyLimitEnabled
                ? dto.DailyEndNumber
                : null;
            fomular.sequenceMode = dto.SequenceMode;
            fomular.formatSource = dto.FormatSource;

            if (dto.LimitDay.HasValue)
                fomular.limitDay = dto.LimitDay.Value.ToUniversalTime();

            await _context.SaveChangesAsync();

            return "Cập nhật công thức thành công.";
        }

        public async Task<string> Preview(int serviceId)
        {
            // 1. Lấy dịch vụ
            var service = await _context.services
                .FirstOrDefaultAsync(x => x.id == serviceId);

            if (service == null)
                throw new Exception("Không tìm thấy dịch vụ.");

            // 2. Lấy công thức
            var formula = await _context.numberFomulars
                .FirstOrDefaultAsync(x => x.serviceId == service.id);

            if (formula == null)
                throw new Exception("Dịch vụ chưa được cấu hình công thức cấp số.");

            if (formula.startNumber < 0 ||
                formula.startNumber > formula.endNumber)
                throw new Exception("Khoảng số không hợp lệ.");

            // 3. Xác định ngày hiện tại theo giờ Việt Nam
            var vietnamNow = DateTimeOffset.UtcNow
                .ToOffset(TimeSpan.FromHours(7));

            var startOfDay = new DateTimeOffset(
                vietnamNow.Year,
                vietnamNow.Month,
                vietnamNow.Day,
                0, 0, 0,
                TimeSpan.FromHours(7));

            var endOfDay = startOfDay.AddDays(1);

            int nextNumber;

            // 4. Independent
            if (formula.sequenceMode == "Independent")
            {
                var issuedIds = await _context.stickers
                    .Where(x => x.serviceId == service.id)
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
                    ? Math.Max(formula.startNumber, issuedNumbers.Max() + 1)
                    : formula.startNumber;

                if (nextNumber > formula.endNumber)
                    throw new Exception("Đã hết số trong khoảng cấu hình.");
            }
            // 5. Shared
            else if (formula.sequenceMode == "Shared")
            {
                var sharedServiceIds = await _context.numberFomulars
                    .Where(x => x.sequenceMode == "Shared")
                    .Select(x => x.serviceId)
                    .ToListAsync();

                var issuedIds = await _context.stickers
                    .Where(x =>
                        sharedServiceIds.Contains(x.serviceId) &&
                        x.issuanceTime >= startOfDay.ToUniversalTime() &&
                        x.issuanceTime < endOfDay.ToUniversalTime())
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
                    ? Math.Max(formula.startNumber, issuedNumbers.Max() + 1)
                    : formula.startNumber;

                if (nextNumber > formula.endNumber)
                    throw new Exception("Đã hết số trong khoảng cấu hình.");

                if (formula.isDailyLimitEnabled)
                {
                    if (!formula.dailyEndNumber.HasValue)
                        throw new Exception("Chưa cấu hình giới hạn số trong ngày.");

                    if (nextNumber > formula.dailyEndNumber.Value)
                        throw new Exception("Đã hết số trong ngày.");
                }
            }
            else
            {
                throw new Exception("Chế độ cấp số không hợp lệ.");
            }

            // 6. Tạo phần số
            string numberText = nextNumber.ToString("D5");

            // 7. Xử lý Prefix / Suffix
            string prefix = "";
            string suffix = "";

            if (formula.formatSource == "ServiceCode")
            {
                string serviceCode = new string(
                    service.serviceId
                        .Where(c => !char.IsWhiteSpace(c))
                        .ToArray());

                string code = serviceCode.Length > 3
                    ? serviceCode.Substring(0, 3)
                    : serviceCode;

                if (formula.numberType == "Prefix")
                    prefix = code;
                else if (formula.numberType == "Suffix")
                    suffix = code;
                else
                    throw new Exception("Loại định dạng không hợp lệ.");
            }
            else if (formula.formatSource == "Custom")
            {
                if (formula.numberType == "Prefix")
                    prefix = formula.firstName;
                else if (formula.numberType == "Suffix")
                    suffix = formula.lastName;
                else
                    throw new Exception("Loại định dạng không hợp lệ.");
            }
            else
            {
                throw new Exception("Nguồn định dạng không hợp lệ.");
            }

            return prefix + numberText + suffix;
        }
        //public Task<bool> ConvertToShared(List<int> serviceIds)
        //{
        //    return Task.FromResult(false);
        //}

        //public string ReSet(int formulaId)
        //{
        //    return "Chưa triển khai reset.";
        //}

        public async Task<List<NumberFomulaListDto>> GetAll()
        {
            return await _context.numberFomulars
                .Select(x => new NumberFomulaListDto
                {
                    Id = x.id,
                    ServiceId = x.serviceId,
                    ServiceName = x.Service.serviceName,
                    NumberType = x.numberType,
                    FirstName = x.firstName,
                    LastName = x.lastName,
                    StartNumber = x.startNumber,
                    EndNumber = x.endNumber,
                    IsDailyLimitEnabled = x.isDailyLimitEnabled,
                    DailyEndNumber = x.dailyEndNumber,
                    LimitDay = x.limitDay,
                    SequenceMode = x.sequenceMode,
                    FormatSource = x.formatSource
                })
                .ToListAsync();
        }

        public async Task<string> Delete(int id)
        {
            var fomular = await _context.numberFomulars.FirstOrDefaultAsync(x => x.id == id);

            if(fomular == null)
                return "Không tìm thấy công thức.";

            bool hasStickers = await _context.stickers.AnyAsync(x => x.serviceId == fomular.serviceId);

            if (hasStickers)
                return "Công thức đang được sử dụng, không thể xóa.";

            _context.numberFomulars.Remove(fomular);
            await _context.SaveChangesAsync();

            return "Xóa công thức thành công.";
        }

    }
}
