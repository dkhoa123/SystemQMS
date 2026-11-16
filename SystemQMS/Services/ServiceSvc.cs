using Microsoft.EntityFrameworkCore;
using SystemQMS.Constants;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.PageResultDto;
using SystemQMS.ViewModels.ServiceDto;

namespace SystemQMS.Services
{
    public class ServiceSvc : IServiceSvc
    {
        private readonly MyDbContext _context;

        public ServiceSvc(MyDbContext context)
        {
            _context = context;
        }

        // SVC-01: Lấy danh sách dịch vụ kèm bộ lọc
        public async Task<PageResultDto<ServiceResponseDto>> GetAllServicesAsync(FilterServiceDto filter)
        {
            // 1. Query ban đầu
            var query = _context.Services
                .AsNoTracking()
                .AsQueryable();

            // 2. Lọc theo keyword
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var keyword = filter.keyword.Trim().ToLower();

                query = query.Where(s =>
                    s.ServiceName.ToLower().Contains(keyword) ||
                    s.ServiceId.ToLower().Contains(keyword));
            }

            // 3. Lọc trạng thái
            if (!string.IsNullOrEmpty(filter.status))
            {
                query = query.Where(s =>
                    s.Status == filter.status);
            }

            // 4. Lọc từ ngày
            if (filter.fromDate.HasValue)
            {
                query = query.Where(s =>
                    s.CreatedAt >= filter.fromDate.Value);
            }

            // 5. Lọc đến ngày (bao gồm toàn bộ ngày được chọn)
            if (filter.toDate.HasValue)
            {
                var endDate = filter.toDate.Value.Date.AddDays(1);

                query = query.Where(s =>
                    s.CreatedAt < endDate);
            }

            // 6. Đếm tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();

            // 7. Phân trang
            int page = Math.Max(1, filter.page);
            int pageSize = Math.Max(1, filter.pageSize);

            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / pageSize
            );

            // 8. Lấy dữ liệu và các bảng liên quan
            var services = await query
                .Include(s => s.Counters)

                .Include(s => s.Schedules)
                    .ThenInclude(sc => sc.Shifts)
                        .ThenInclude(ss => ss.SheduleActive)

                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 9. Mapping Entity sang DTO
            var data = services.Select(s => new ServiceResponseDto
            {
                id = s.Id,

                serviceCode = s.ServiceId,

                serviceName = s.ServiceName,

                description = s.Description,

                status = s.Status,

                createdAt = s.CreatedAt,

                assignmentMethod = s.AssignmentMethod,

                imageUrl = s.Image,

                // Danh sách quầy thuộc dịch vụ
                counterIds = s.Counters
                    .Select(c => c.CounterId.ToString())
                    .ToList(),

                // Lịch hoạt động theo thứ
                schedule = s.Schedules.ToDictionary(
                    sc => sc.DayOfWeek,
                    sc => new ServiceScheduleDayDto
                    {
                        enabled = sc.Enabled,

                        startTime = sc.StartTime.HasValue
                            ? sc.StartTime.Value.ToString(@"hh\:mm")
                            : null,

                        endTime = sc.EndTime.HasValue
                            ? sc.EndTime.Value.ToString(@"hh\:mm")
                            : null,

                        shiftIds = sc.Shifts
                            .Select(ss => ss.SheduleActiveId.ToString())
                            .ToList()
                    }
                )

            }).ToList();

            // 10. Trả kết quả
            return new PageResultDto<ServiceResponseDto>
            {
                items = data,
                page = page,
                pageSize = pageSize,
                total = totalPages
            };
        }


        // SVC-05: Xem chi tiết dịch vụ đầy đủ (bao gồm quầy tiếp nhận và danh sách STT)
        public async Task<ServiceDetailDto?> GetServiceByIdAsync(int id)
        {
            var service = await _context.Services
                .AsNoTracking()
                .Include(s => s.Counters)
                .Include(s => s.Schedules)
                    .ThenInclude(sc => sc.Shifts)
                        .ThenInclude(ss => ss.SheduleActive)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return null;

            return new ServiceDetailDto
            {
                id = service.Id,

                serviceCode = service.ServiceId,

                serviceName = service.ServiceName,

                description = service.Description,

                status = service.Status,

                createdAt = service.CreatedAt,

                assignmentMethod = service.AssignmentMethod,

                imageUrl = service.Image,

                // Danh sách quầy thuộc dịch vụ
                counterIds = service.Counters
                    .Select(c => c.CounterId.ToString())
                    .ToList(),

                // Lịch hoạt động theo thứ
                schedule = service.Schedules.ToDictionary(
                    sc => sc.DayOfWeek,
                    sc => new ServiceScheduleDayDto
                    {
                        enabled = sc.Enabled,

                        startTime = sc.StartTime.HasValue
                            ? sc.StartTime.Value.ToString(@"hh\:mm")
                            : null,

                        endTime = sc.EndTime.HasValue
                            ? sc.EndTime.Value.ToString(@"hh\:mm")
                            : null,

                        shiftIds = sc.Shifts
                            .Select(ss => ss.SheduleActiveId.ToString())
                            .ToList()
                    }
                )

            };
        }

        public async Task<PageResultDto<ServiceHistoryDto>> GetServiceByIdAsync(int id, FilterServiceDto filter)
        {
            var query = _context.Tickets
                .AsNoTracking()
                .Where(t => t.ServiceId == id);

            // 2. Lọc theo keyword
            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var keyword = filter.keyword.Trim().ToLower();

                query = query.Where(s =>
                    s.SttId.ToLower().Contains(keyword) ||
                    s.Counter.CounterName.ToLower().Contains(keyword));
            }

            // 3. Lọc trạng thái
            if (!string.IsNullOrEmpty(filter.status))
            {
                query = query.Where(s =>
                    s.Status == filter.status);
            }

            // 4. Lọc từ ngày
            if (filter.fromDate.HasValue)
            {
                query = query.Where(s =>
                    s.IssuanceTime >= filter.fromDate.Value);
            }

            // 5. Lọc đến ngày (bao gồm toàn bộ ngày được chọn)
            if (filter.toDate.HasValue)
            {
                var endDate = filter.toDate.Value.Date.AddDays(1);

                query = query.Where(s =>
                    s.IssuanceTime < endDate);
            }

            // 6. Đếm tổng số bản ghi sau khi lọc
            int totalRecords = await query.CountAsync();

            // 7. Phân trang
            int page = Math.Max(1, filter.page);
            int pageSize = Math.Max(1, filter.pageSize);

            int totalPages = (int)Math.Ceiling(
                (double)totalRecords / pageSize
            );

            // 8. Lấy dữ liệu và các bảng liên quan
            var tickets = await query
                .Include(t => t.Counter)
                .Include(s => s.Service)
                .Include(u => u.Users)
                .OrderByDescending(t => t.IssuanceTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = tickets.Select(t => new ServiceHistoryDto
            {
                id = t.SttId, // Khớp với rowKey="id" ở Table
                serviceCode = t.Service?.ServiceId ?? "",
                ticketNumber = t.TicketNumber.ToString(),
                status = t.Status,
                performedBy = t.Users?.Username ?? "-",
                usedAt = t.IssuanceTime.ToString("yyyy-MM-dd HH:mm:ss"), // Sửa useAt -> usedAt
                counterName = t.Counter?.CounterName ?? "-"
            }).ToList();

            return new PageResultDto<ServiceHistoryDto>
            {
                items = data,
                page = page,
                pageSize = pageSize,
                total = totalRecords
            };

        }

        // SVC-02 & SVC-03: Thêm mới dịch vụ
        //public async Task<(bool Success, string Message, int? NewId, string? ServiceId)> CreateServiceAsync(CreateServiceDto dto)
        //{
        //    bool unitExists = await _context.Units.AnyAsync(u => u.UnitId == dto.unitId);
        //    if (!unitExists)
        //        return (false, $"Đơn vị có ID = {dto.unitId} không tồn tại trong hệ thống.", null, null);

        //    bool isDuplicate = await _context.Services.AnyAsync(s =>
        //        s.UnitId == dto.unitId && s.ServiceId.ToLower() == dto.serviceId.Trim().ToLower());

        //    if (isDuplicate)
        //        return (false, $"Mã dịch vụ '{dto.serviceId}' đã tồn tại trong đơn vị.", null, null);

        //    var service = new Service
        //    {
        //        UnitId = dto.unitId,
        //        ServiceId = dto.serviceId.Trim(),
        //        ServiceName = dto.serviceName.Trim(),
        //        Description = dto.description,
        //        Image = dto.image,
        //        IsActive = dto.isActive,
        //        SharingLink = dto.sharingLink,
        //        AverageTime = dto.averageTime
        //    };

        //    _context.Services.Add(service);
        //    await _context.SaveChangesAsync();

        //    return (true, "Thêm dịch vụ thành công.", service.Id, service.ServiceId);
        //}

        // SVC-04: Cập nhật thông tin dịch vụ
        public async Task<ServiceResponseDto> UpdateServiceAsync(int id, UpdateServiceDto dto)
        {
            // 1. Include sẵn Schedules và Shifts để EF Core theo dõi (tracking) thay đổi
            var service = await _context.Services
                .Include(s => s.Schedules)
                    .ThenInclude(sc => sc.Shifts)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                throw new Exception($"Không tìm thấy dịch vụ có ID = {id}");
            }

            // 2. Cập nhật thông tin cơ bản
            service.ServiceId = dto.serviceCode;
            service.ServiceName = dto.serviceName;
            service.Description = dto.description;
            service.Status = dto.status;
            if (!string.IsNullOrWhiteSpace(dto.assignmentMethod))
            {
                service.AssignmentMethod = dto.assignmentMethod;
            }
            else if (string.IsNullOrWhiteSpace(service.AssignmentMethod))
            {
                service.AssignmentMethod = "ASSIGN_ON_ISSUE";
            }

            if (!string.IsNullOrWhiteSpace(dto.imageUrl))
            {
                var base64Data = dto.imageUrl.Contains(",")
                    ? dto.imageUrl.Split(',')[1]
                    : dto.imageUrl;
                service.Image = Convert.FromBase64String(base64Data);
            }

            // 3. Cập nhật lịch hoạt động (Schedule)
            if (dto.schedule != null)
            {
                foreach (var entry in dto.schedule)
                {
                    var dayOfWeek = entry.Key.Trim(); // VD: "MONDAY", "TUESDAY"...
                    var dayDto = entry.Value;

                    // Tìm bản ghi ngày trong DB (so sánh không phân biệt hoa thường)
                    var existingSchedule = service.Schedules.FirstOrDefault(sc =>
                        sc.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase));

                    if (existingSchedule == null)
                    {
                        existingSchedule = new ServiceSchedule
                        {
                            ServiceId = service.Id,
                            DayOfWeek = dayOfWeek,
                            Shifts = new List<ServiceScheduleShift>()
                        };
                        service.Schedules.Add(existingSchedule);
                    }

                    existingSchedule.Enabled = dayDto.enabled;

                    // Parse StartTime & EndTime từ string ("HH:mm" hoặc "HH:mm:ss") sang TimeSpan?
                    existingSchedule.StartTime = dayDto.enabled && TimeSpan.TryParse(dayDto.startTime, out var start)
                        ? start
                        : null;

                    existingSchedule.EndTime = dayDto.enabled && TimeSpan.TryParse(dayDto.endTime, out var end)
                        ? end
                        : null;

                    // Xóa toàn bộ ca cũ của ngày này để ghi lại danh sách ca mới
                    existingSchedule.Shifts.Clear();

                    if (dayDto.enabled && dayDto.shiftIds != null && dayDto.shiftIds.Count > 0)
                    {
                        // Distinct() để tránh lỗi Unique Index (ScheduleId, SheduleActiveId) nếu FE gửi trùng ID
                        var validShiftIds = dayDto.shiftIds
                            .Select(shiftIdStr => int.TryParse(shiftIdStr, out var shiftId) ? shiftId : (int?)null)
                            .Where(shiftId => shiftId.HasValue)
                            .Select(shiftId => shiftId!.Value)
                            .Distinct();

                        foreach (var shiftId in validShiftIds)
                        {
                            existingSchedule.Shifts.Add(new ServiceScheduleShift
                            {
                                SheduleActiveId = shiftId
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return new ServiceResponseDto
            {
                id = service.Id,
                serviceCode = service.ServiceId,
                serviceName = service.ServiceName,
                description = service.Description,
                status = service.Status,
                createdAt = service.CreatedAt,
                assignmentMethod = service.AssignmentMethod,
                schedule = service.Schedules.ToDictionary(
                    sc => sc.DayOfWeek,
                    sc => new ServiceScheduleDayDto
                    {
                        enabled = sc.Enabled,
                        startTime = sc.StartTime.HasValue ? sc.StartTime.Value.ToString(@"hh\:mm") : null,
                        endTime = sc.EndTime.HasValue ? sc.EndTime.Value.ToString(@"hh\:mm") : null,
                        shiftIds = sc.Shifts.Select(ss => ss.SheduleActiveId.ToString()).ToList()
                    }
                ),
                imageUrl = service.Image
            };
        }

        // SVC-06: Xóa dịch vụ (Bảo vệ toàn vẹn lịch sử vé)
        public async Task<(bool Success, string Message)> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {id}");

            bool hasTickets = await _context.Tickets.AnyAsync(t => t.ServiceId == service.Id);
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.ServiceId == service.Id);

            if (hasTickets || hasBookings)
                return (false, "Không thể xóa dịch vụ này vì đã có phiếu STT hoặc lịch hẹn phát sinh. Hãy chuyển trạng thái sang 'Ngưng hoạt động'.");

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return (true, "Xóa dịch vụ thành công.");
        }

        
    }
}