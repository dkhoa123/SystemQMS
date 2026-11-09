using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.CrmDto;

namespace SystemQMS.Services
{
    public class GuestSvc : IGuestSvc
    {
        private readonly MyDbContext _context;

        public GuestSvc(MyDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<GuestDto>> GetGuestsAsync(GuestQueryFilterDto filter)
        {
            var query = _context.guests
                .Include(g => g.Unit)
                .AsNoTracking()
                .AsQueryable();

            if (filter.unitId.HasValue && filter.unitId.Value > 0)
            {
                query = query.Where(g => g.unitId == filter.unitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.status))
            {
                query = query.Where(g => g.status == filter.status.Trim());
            }

            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(g =>
                    (g.guestName != null && g.guestName.ToLower().Contains(kw)) ||
                    (g.phoneNumber != null && g.phoneNumber.Contains(kw)) ||
                    g.CCCD.ToLower().Contains(kw) ||
                    g.guestId.ToLower().Contains(kw));
            }

            var totalItems = await query.CountAsync();

            var page = filter.page <= 0 ? 1 : filter.page;
            var pageSize = filter.pageSize <= 0 ? 10 : filter.pageSize;

            var items = await query
                .OrderByDescending(g => g.registerTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GuestDto
                {
                    id = g.id,
                    unitId = g.unitId,
                    unitName = g.Unit != null ? g.Unit.unitName : string.Empty,
                    guestId = g.guestId,
                    guestName = g.guestName,
                    phoneNumber = g.phoneNumber,
                    email = g.email,
                    CCCD = g.CCCD,
                    registerTime = g.registerTime,
                    status = g.status
                })
                .ToListAsync();

            return new PagedResultDto<GuestDto>
            {
                totalItems = totalItems,
                page = page,
                pageSize = pageSize,
                items = items
            };
        }

        public async Task<GuestDto?> GetGuestByIdAsync(int id)
        {
            var guest = await _context.guests
                .Include(g => g.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.id == id);

            if (guest == null) return null;

            return MapToDto(guest);
        }

        public async Task<GuestDto?> GetGuestByCccdAsync(string cccd)
        {
            var guest = await _context.guests
                .Include(g => g.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.CCCD == cccd.Trim());

            if (guest == null) return null;

            return MapToDto(guest);
        }

        public async Task<GuestDto> CreateGuestAsync(CreateGuestRequestDto request)
        {
            // 1. Kiểm tra và nạp thực thể Unit để tránh null unitName
            var unit = await _context.units.FirstOrDefaultAsync(u => u.unitId == request.unitId);
            if (unit == null)
            {
                throw new KeyNotFoundException($"Đơn vị với unitId = {request.unitId} không tồn tại.");
            }

            // 2. Chặn trùng CCCD
            var cccdExists = await _context.guests.AnyAsync(g => g.CCCD == request.CCCD.Trim());
            if (cccdExists)
            {
                throw new InvalidOperationException($"Số CCCD '{request.CCCD}' đã được đăng ký trong hệ thống.");
            }

            // 3. Tự sinh guestId nếu không truyền
            var finalGuestId = string.IsNullOrWhiteSpace(request.guestId)
                ? $"KH{DateTimeOffset.UtcNow:yyMMddHHmmss}"
                : request.guestId.Trim();

            // 4. Mật khẩu mặc định nếu để trống
            var finalPassword = string.IsNullOrWhiteSpace(request.password) ? "123456" : request.password.Trim();

            var newGuest = new Guest
            {
                unitId = request.unitId,
                guestId = finalGuestId,
                guestName = request.guestName.Trim(),
                phoneNumber = request.phoneNumber?.Trim(),
                email = request.email?.Trim(),
                CCCD = request.CCCD.Trim(),
                password = finalPassword,
                registerTime = DateTimeOffset.UtcNow,
                status = string.IsNullOrWhiteSpace(request.status) ? "Active" : request.status.Trim(),
                Unit = unit
            };

            _context.guests.Add(newGuest);
            await _context.SaveChangesAsync();

            return MapToDto(newGuest);
        }

        public async Task<GuestDto> UpdateGuestAsync(int id, UpdateGuestRequestDto request)
        {
            // Nạp kèm thông tin Unit để sau khi update map ra DTO không bị null unitName
            var guest = await _context.guests
                .Include(g => g.Unit)
                .FirstOrDefaultAsync(g => g.id == id);

            if (guest == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy khách hàng với id = {id}.");
            }

            // Chặn trùng CCCD với tài khoản khác
            var cccdDuplicate = await _context.guests
                .AnyAsync(g => g.id != id && g.CCCD == request.CCCD.Trim());
            if (cccdDuplicate)
            {
                throw new InvalidOperationException($"Số CCCD '{request.CCCD}' đã được sử dụng bởi một khách hàng khác.");
            }

            guest.guestName = request.guestName.Trim();
            guest.phoneNumber = request.phoneNumber?.Trim();
            guest.email = request.email?.Trim();
            guest.CCCD = request.CCCD.Trim();

            if (!string.IsNullOrWhiteSpace(request.status))
            {
                guest.status = request.status.Trim();
            }

            await _context.SaveChangesAsync();
            return MapToDto(guest);
        }

        public async Task DeleteGuestAsync(int id)
        {
            var guest = await _context.guests.FirstOrDefaultAsync(g => g.id == id);
            if (guest == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy khách hàng với id = {id}.");
            }

            guest.status = "Inactive";
            await _context.SaveChangesAsync();
        }

        private static GuestDto MapToDto(Guest guest)
        {
            return new GuestDto
            {
                id = guest.id,
                unitId = guest.unitId,
                unitName = guest.Unit?.unitName,
                guestId = guest.guestId,
                guestName = guest.guestName,
                phoneNumber = guest.phoneNumber,
                email = guest.email,
                CCCD = guest.CCCD,
                registerTime = guest.registerTime,
                status = guest.status
            };
        }
    }
}