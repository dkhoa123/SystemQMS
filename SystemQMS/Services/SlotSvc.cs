using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.IntergrationsDto;

namespace SystemQMS.Services
{
    public class SlotSvc : ISlotSvc
    {
        private readonly MyDbContext _context;

        public SlotSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<ReserveSlotResponse> ReserveSlotAsync(int unitId, ReserveSlotRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(global::System.Data.IsolationLevel.Serializable);

            try
            {
                var unitConfig = await _context.unitConfigs.FirstOrDefaultAsync(u => u.UnitId == unitId);
                if (unitConfig == null)
                {
                    return new ReserveSlotResponse { IsSuccess = false, Message = "Không tìm thấy cấu hình đơn vị." };
                }

                int maxGuests = unitConfig.MaxGuestsPerSlot ?? 0;
                if (maxGuests <= 0)
                {
                    return new ReserveSlotResponse { IsSuccess = false, Message = "Dịch vụ chưa cấu hình số lượt tối đa." };
                }

                var currentBookingsCount = await _context.bookings
                    .Where(b => b.serviceId == request.ServiceId &&
                                b.timeDate == request.TimeDate &&
                                b.status != "Cancelled" && b.status != "Expired")
                    .CountAsync();

                if (currentBookingsCount >= maxGuests)
                {
                    return new ReserveSlotResponse { IsSuccess = false, Message = "Khung giờ này đã hết lượt. Vui lòng chọn khung giờ khác." };
                }

                if (unitConfig.MaxBookingsPerUserDaily.HasValue)
                {
                    var userDailyBookings = await _context.bookings
                        .Where(b => b.guestId == request.GuestId && b.Date.Date == request.Date.Date && b.status != "Cancelled")
                        .CountAsync();

                    if (userDailyBookings >= unitConfig.MaxBookingsPerUserDaily.Value)
                    {
                        return new ReserveSlotResponse { IsSuccess = false, Message = "Bạn đã đạt giới hạn lượt bốc số trong ngày." };
                    }
                }

                var newBooking = new Booking
                {
                    guestId = request.GuestId,
                    serviceId = request.ServiceId,
                    Date = request.Date,
                    timeDate = request.TimeDate,
                    status = "Reserved",
                    sttId = "TEMP_" + Guid.NewGuid().ToString().Substring(0, 8),
                    ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10)
                };

                _context.bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new ReserveSlotResponse
                {
                    IsSuccess = true,
                    Message = "Giữ lượt thành công.",
                    BookingId = newBooking.id
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ReserveSlotResponse { IsSuccess = false, Message = $"Đã xảy ra lỗi: {ex.Message}" };
            }
        }
    }
}