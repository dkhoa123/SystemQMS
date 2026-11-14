using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.KioskDto;

namespace SystemQMS.Services
{
    public class KioskSvc : IKioskSvc
    {
        private MyDbContext _context;
        public KioskSvc(MyDbContext context)
        {
            _context = context;
        }

        public List<ServiceKioskDto> GetServiceKiosks(string kioskDeviceId)
        {
            var today = DateTimeOffset.Now.Date;

            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;

            // Truy xuất trực tiếp từ bảng services và map luôn ra DTO trong 1 câu Query duy nhất
            var serviceKiosks = _context.Services
                // 1. Lọc các dịch vụ thuộc Kiosk này và đang hoạt động
                .Where(s => s.Devices.Any(d => d.DeviceId == kioskDeviceId) 
                            && s.IsActive == true)
                .Select(s => new
                {
                    Service = s,
                    // 1. KSK-07: Kiểm tra xem có lịch làm việc cho dịch vụ này không
                    HashWorkingShedule = _context.Shedules.Any(sh =>
                    sh.ServiceId == s.Id && 
                    sh.StartTime <= currentTime && 
                    sh.EndTime >= currentTime),

                    // 2. KSK-01: Kiểm tra xem có quầy nào đang mở cho dịch vụ này không
                    HasOpenCounter = s.Counters.Any(c => 
                    c.IsActive == true &&
                    c.Status == "Open"),

                    // 3. KSK-01: STT cấp gần nhất trong ngày
                    LatestStickerId = _context.Tickets
                        .Where(st => st.ServiceId == s.Id && st.IssuanceTime.Date == today)
                        .OrderByDescending(st => st.IssuanceTime)
                        .Select(st => st.SttId)
                        .FirstOrDefault(),

                    // 4. KSK-03: Tính số lượng vé đang chờ
                    WaitingCount = _context.Tickets
                        .Count(st => st.ServiceId == s.Id && st.IssuanceTime.Date == today && st.Status == "Đang chờ")
                })
                .ToList();

            // Map kết quả ra List<ServiceKioskDto>
            var resultList = serviceKiosks.Select(data => new ServiceKioskDto
            {
                serviceId = data.Service.ServiceId,
                serviceName = data.Service.ServiceName,
                statusActive = "Hoạt động", // Đã lọc isActive == true ở trên
                isDisabled = !data.HashWorkingShedule,
                isBlurred = data.HashWorkingShedule && !data.HasOpenCounter,
                LastSTT = data.LatestStickerId ?? "Chưa có",
                lastMinutes = data.Service.AverageTime * data.WaitingCount
            }).ToList();

            return resultList;
        }

        public async Task<TicketReceiptDto> CreateTicketAsync(int serviceId, int counterId, int userId)
        {
            var now = DateTimeOffset.Now;
            var today = now.Date;
            var endOfDay = today.AddDays(1).AddTicks(-1);

            // Dùng Transaction để khóa dòng, chống cấp trùng số khi bốc số đồng thời
            using var transaction = await _context.Database.BeginTransactionAsync(global::System.Data.IsolationLevel.Serializable); try
            {
                var nay = DateTime.Now.TimeOfDay;

                var hasWorkingSchedule = await _context.Shedules.AnyAsync(sh =>
                    sh.ServiceId == serviceId &&
                    sh.StartTime <= nay &&
                    sh.EndTime > nay
                );

                if (!hasWorkingSchedule)
                    throw new Exception("Dịch vụ hiện đang ngoài giờ hoạt động");

                var hasOpenCounter = await _context.Counters.AnyAsync(c =>
                    c.Services.Any(s => s.Id == serviceId) &&
                    c.IsActive &&
                    c.Status == "Open"
                );

                if (!hasOpenCounter)
                    throw new Exception("Hiện không có quầy tiếp nhận dịch vụ");
                var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
                if (service == null || !service.IsActive) throw new Exception("Dịch vụ không khả dụng");

                // 1. Lấy cấu hình dãy số
                var rule = await _context.NumberFomulars.FirstOrDefaultAsync(r => r.ServiceId == serviceId);
                if (rule == null) throw new Exception("Dịch vụ chưa thiết lập quy tắc cấp số"); // TKT-06

                // 2. Lấy số thứ tự (STT) gần nhất trong ngày
                // Tuỳ thuộc rule là dãy Chung hay Riêng mà điều kiện truy vấn tickets sẽ khác nhau. 
                // Giả sử lấy số cao nhất hiện tại của dịch vụ:
                var lastNumber = await _context.Tickets
                    .Where(st => st.ServiceId == serviceId &&
                                 st.IssuanceTime.Date == today)
                    .MaxAsync(st => (int?)st.TicketNumber) ?? rule.StartNumber - 1;

                int nextNumber = lastNumber + 1;

                // Kiểm tra hết số (TKT-05)
                if (nextNumber > rule.EndNumber)
                {
                    throw new Exception($"Dịch vụ {service.ServiceName} đã cấp hết số thứ tự, vui lòng chọn dịch vụ khác hoặc quay lại sau");
                }

                // Định dạng STT: Tiền tố (firstName) + Số + Hậu tố (lastName)
                string formattedNumber = nextNumber.ToString("D3");
                string fullTicketNumber = $"{rule.FirstName}{formattedNumber}{rule.LastName}".Trim();

                // 3. KSK-03: Tính thời gian chờ dự kiến
                int? estimatedWait = null;
                if (service.AverageTime > 0) // Kiểm tra nếu có cài thời gian xử lý
                {
                    var waitingCount = await _context.Tickets
                        .CountAsync(st => st.ServiceId == serviceId && st.Status == "Đang chờ" && st.IssuanceTime.Date == today);

                    estimatedWait = service.AverageTime * waitingCount;
                }

                var counter = await _context.Counters
                         .FirstOrDefaultAsync(c =>
                              c.Id == counterId &&
                              c.IsActive &&
                              c.Status == "Open" &&
                              c.Services.Any(s => s.Id == serviceId)
                          );

                if (counter == null)
                {
                    throw new Exception("Quầy không tồn tại hoặc hiện không thể tiếp nhận dịch vụ này");
                }
                var userExists = await _context.Users
                    .AnyAsync(u => u.UserId == userId);

                if (!userExists)
                {
                    throw new Exception("Người dùng không tồn tại");
                }

                // 4. Lưu dữ liệu
                var newSticker = new Tickets
                {
                    SttId = fullTicketNumber,
                    UnitId = service.UnitId,
                    ServiceId = service.Id,
                    IssuanceTime = now.ToUniversalTime(),
                    UseTime = endOfDay.ToUniversalTime(),
                    CounterId = counter.Id,
                    UserId = userId,
                    Status = "Đang chờ", // Giai đoạn khởi tạo
                    Stage = "chỉ định quầy" // Hoặc Đã chỉ định quầy tùy chế độ chia
                };

                _context.Tickets.Add(newSticker);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TicketReceiptDto
                {
                    ticketNumber = fullTicketNumber,
                    serviceName = service.ServiceName,
                    issuanceTime = newSticker.IssuanceTime,
                    expiryTime = newSticker.UseTime,
                    waitMinute = estimatedWait,
                    // Link giả định truyền STT để làm màn hình đánh giá quét QR
                    QrCodeData = $"https://yourdomain.com/evaluate?stt={fullTicketNumber}&unit={service.UnitId}"
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public bool ReportDeviceStatus(string kioskDeviceId, string status)
        {
            var device = _context.Devices.FirstOrDefault(d => d.DeviceId == kioskDeviceId);
            if (device != null)
            {

                device.StatusConnect = status;
                // Nếu lỗi in, theo tài liệu KSK-05, Kiosk tự chuyển sang màn hình bảo trì.
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
