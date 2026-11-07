using DocumentFormat.OpenXml.Office2016.Excel;
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
            var serviceKiosks = _context.services
                // 1. Lọc các dịch vụ thuộc Kiosk này và đang hoạt động
                .Where(s => s.Devices.Any(d => d.deviceId == kioskDeviceId) 
                            && s.isActive == true)
                .Select(s => new
                {
                    Service = s,
                    // 1. KSK-07: Kiểm tra xem có lịch làm việc cho dịch vụ này không
                    HashWorkingShedule = _context.shedules.Any(sh =>
                    sh.serviceId == s.id && 
                    sh.startTime <= currentTime && 
                    sh.endTime >= currentTime),

                    // 2. KSK-01: Kiểm tra xem có quầy nào đang mở cho dịch vụ này không
                    HasOpenCounter = s.counters.Any(c => 
                    c.isActive == true &&
                    c.status == "Open"),

                    // 3. KSK-01: STT cấp gần nhất trong ngày
                    LatestStickerId = _context.stickers
                        .Where(st => st.serviceId == s.id && st.issuanceTime.Date == today)
                        .OrderByDescending(st => st.issuanceTime)
                        .Select(st => st.sttId)
                        .FirstOrDefault(),

                    // 4. KSK-03: Tính số lượng vé đang chờ
                    WaitingCount = _context.stickers
                        .Count(st => st.serviceId == s.id && st.issuanceTime.Date == today && st.status == "Đang chờ")
                })
                .ToList();

            // Map kết quả ra List<ServiceKioskDto>
            var resultList = serviceKiosks.Select(data => new ServiceKioskDto
            {
                serviceId = data.Service.serviceId,
                serviceName = data.Service.serviceName,
                statusActive = "Hoạt động", // Đã lọc isActive == true ở trên
                isDisabled = !data.HashWorkingShedule,
                isBlurred = data.HashWorkingShedule && !data.HasOpenCounter,
                LastSTT = data.LatestStickerId ?? "Chưa có",
                lastMinutes = data.Service.averageTime * data.WaitingCount
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

                var hasWorkingSchedule = await _context.shedules.AnyAsync(sh =>
                    sh.serviceId == serviceId &&
                    sh.startTime <= nay &&
                    sh.endTime > nay
                );

                if (!hasWorkingSchedule)
                    throw new Exception("Dịch vụ hiện đang ngoài giờ hoạt động");

                var hasOpenCounter = await _context.counters.AnyAsync(c =>
                    c.Services.Any(s => s.id == serviceId) &&
                    c.isActive &&
                    c.status == "Open"
                );

                if (!hasOpenCounter)
                    throw new Exception("Hiện không có quầy tiếp nhận dịch vụ");
                var service = await _context.services.FirstOrDefaultAsync(s => s.id == serviceId);
                if (service == null || !service.isActive) throw new Exception("Dịch vụ không khả dụng");

                // 1. Lấy cấu hình dãy số
                var rule = await _context.numberFomulars.FirstOrDefaultAsync(r => r.serviceId == serviceId);
                if (rule == null) throw new Exception("Dịch vụ chưa thiết lập quy tắc cấp số"); // TKT-06

                // 2. Lấy số thứ tự (STT) gần nhất trong ngày
                // Tuỳ thuộc rule là dãy Chung hay Riêng mà điều kiện truy vấn stickers sẽ khác nhau. 
                // Giả sử lấy số cao nhất hiện tại của dịch vụ:
                var lastNumber = await _context.stickers
                    .Where(st => st.serviceId == serviceId &&
                                 st.issuanceTime.Date == today)
                    .MaxAsync(st => (int?)st.ticketNumber) ?? rule.startNumber - 1;

                int nextNumber = lastNumber + 1;

                // Kiểm tra hết số (TKT-05)
                if (nextNumber > rule.endNumber)
                {
                    throw new Exception($"Dịch vụ {service.serviceName} đã cấp hết số thứ tự, vui lòng chọn dịch vụ khác hoặc quay lại sau");
                }

                // Định dạng STT: Tiền tố (firstName) + Số + Hậu tố (lastName)
                string formattedNumber = nextNumber.ToString("D3");
                string fullTicketNumber = $"{rule.firstName}{formattedNumber}{rule.lastName}".Trim();

                // 3. KSK-03: Tính thời gian chờ dự kiến
                int? estimatedWait = null;
                if (service.averageTime > 0) // Kiểm tra nếu có cài thời gian xử lý
                {
                    var waitingCount = await _context.stickers
                        .CountAsync(st => st.serviceId == serviceId && st.status == "Đang chờ" && st.issuanceTime.Date == today);

                    estimatedWait = service.averageTime * waitingCount;
                }

                var counter = await _context.counters
                         .FirstOrDefaultAsync(c =>
                              c.id == counterId &&
                              c.isActive &&
                              c.status == "Open" &&
                              c.Services.Any(s => s.id == serviceId)
                          );

                if (counter == null)
                {
                    throw new Exception("Quầy không tồn tại hoặc hiện không thể tiếp nhận dịch vụ này");
                }
                var userExists = await _context.users
                    .AnyAsync(u => u.userId == userId);

                if (!userExists)
                {
                    throw new Exception("Người dùng không tồn tại");
                }

                // 4. Lưu dữ liệu
                var newSticker = new Sticker
                {
                    sttId = fullTicketNumber,
                    unitId = service.unitId,
                    serviceId = service.id,
                    issuanceTime = now.ToUniversalTime(),
                    useTime = endOfDay.ToUniversalTime(),
                    counterId = counter.id,
                    userId = userId,
                    status = "Đang chờ", // Giai đoạn khởi tạo
                    stage = "chỉ định quầy" // Hoặc Đã chỉ định quầy tùy chế độ chia
                };

                _context.stickers.Add(newSticker);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new TicketReceiptDto
                {
                    ticketNumber = fullTicketNumber,
                    serviceName = service.serviceName,
                    issuanceTime = newSticker.issuanceTime,
                    expiryTime = newSticker.useTime,
                    waitMinute = estimatedWait,
                    // Link giả định truyền STT để làm màn hình đánh giá quét QR
                    QrCodeData = $"https://yourdomain.com/evaluate?stt={fullTicketNumber}&unit={service.unitId}"
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
            var device = _context.devices.FirstOrDefault(d => d.deviceId == kioskDeviceId);
            if (device != null)
            {

                device.statusConnect = status;
                // Nếu lỗi in, theo tài liệu KSK-05, Kiosk tự chuyển sang màn hình bảo trì.
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
