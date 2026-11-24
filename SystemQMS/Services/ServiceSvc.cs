using Microsoft.EntityFrameworkCore;
using System.Globalization;
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
        public async Task<ServiceResponseDto> CreateServiceAsync(int unitId, CreateServiceDto dto)
        {
            // 1. Kiểm tra đơn vị
            var unitExists = await _context.Units
                .AnyAsync(u => u.UnitId == unitId);

            if (!unitExists)
                throw new Exception("Đơn vị không tồn tại.");

            // 2. Kiểm tra mã dịch vụ trùng
            var serviceCodeExists = await _context.Services
                .AnyAsync(s => s.ServiceId == dto.serviceCode && s.UnitId == unitId);

            if (serviceCodeExists)
                throw new Exception("Mã dịch vụ đã tồn tại.");

            // 3. Chuẩn hóa dữ liệu
            var status = string.IsNullOrWhiteSpace(dto.status)
                ? "ACTIVE"
                : dto.status.Trim();

            byte[]? imageBytes = null;

            if (!string.IsNullOrWhiteSpace(dto.imageUrl))
            {
                // Hỗ trợ cả data:image/...;base64,xxxx và chuỗi base64 thuần
                var base64 = dto.imageUrl.Contains(',')
                    ? dto.imageUrl[(dto.imageUrl.IndexOf(',') + 1)..]
                    : dto.imageUrl;

                try
                {
                    imageBytes = Convert.FromBase64String(base64);
                }
                catch (FormatException)
                {
                    throw new Exception("Ảnh không đúng định dạng Base64.");
                }
            }

            // 4. Kiểm tra và lấy danh sách quầy
            var requestedCounterIds = (dto.counterIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var counters = await _context.Counters
                .Where(c => requestedCounterIds.Contains(c.CounterId)
                            && c.UnitId == unitId)
                .ToListAsync();

            if (counters.Count != requestedCounterIds.Count)
                throw new Exception("Có quầy không tồn tại hoặc không thuộc đơn vị.");

            // 5. Chuẩn bị danh sách ca làm việc từ shiftIds
            var requestedShiftIds = (dto.schedule ?? new Dictionary<string, ServiceScheduleDayDto>())
                .Values
                .Where(day => day.shiftIds != null)
                .SelectMany(day => day.shiftIds!)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            // Tìm ca làm việc theo mã WordShift
            var existingShifts = await _context.Set<SheduleActive>()
                .Where(s => requestedShiftIds.Contains(s.WordShift))
                .ToListAsync();

            // Kiểm tra mã ca có tồn tại không
            if (existingShifts.Count != requestedShiftIds.Count)
            {
                var foundCodes = existingShifts
                    .Select(s => s.WordShift)
                    .ToHashSet();

                var missingCodes = requestedShiftIds
                    .Where(id => !foundCodes.Contains(id))
                    .ToList();

                throw new Exception(
                    $"Không tìm thấy mã ca làm việc: {string.Join(", ", missingCodes)}");
            }

            // Tạo Dictionary để tra cứu mã ca -> ID database
            var shiftMap = existingShifts.ToDictionary(
                s => s.WordShift,
                s => s.Id
            );


            // 6. Bắt đầu transaction để lưu đồng bộ
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 7. Tạo Service
                var service = new Service
                {
                    UnitId = unitId,
                    ServiceId = dto.serviceCode.Trim(),
                    ServiceName = dto.serviceName.Trim(),
                    Description = dto.description,
                    Status = status,
                    IsActive = status.Equals("ACTIVE",
                        StringComparison.OrdinalIgnoreCase),
                    AssignmentMethod = string.IsNullOrWhiteSpace(dto.assignmentMethod)
                        ? "ASSIGN_ON_ISSUE"
                        : dto.assignmentMethod.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    Image = imageBytes,
                    AverageTime = 0
                };

                // Gán quan hệ nhiều-nhiều với Counter
                foreach (var counter in counters)
                {
                    service.Counters.Add(counter);
                }

                _context.Services.Add(service);

                // Lưu lần đầu để lấy service.Id
                await _context.SaveChangesAsync();

                // 8. Lưu lịch ServiceSchedule và quan hệ ServiceScheduleShift
                if (dto.schedule != null)
                {
                    foreach (var item in dto.schedule)
                    {
                        var dayName = item.Key;
                        var dayDto = item.Value;

                        if (dayName.Length > 20)
                            throw new Exception($"Tên thứ '{dayName}' vượt quá 20 ký tự.");

                        TimeSpan? startTime = null;
                        TimeSpan? endTime = null;

                        if (!string.IsNullOrWhiteSpace(dayDto.startTime))
                        {
                            if (!TimeSpan.TryParse(
                                dayDto.startTime,
                                CultureInfo.InvariantCulture,
                                out var parsedStart))
                            {
                                throw new Exception(
                                    $"Giờ bắt đầu của {dayName} không hợp lệ.");
                            }

                            startTime = parsedStart;
                        }

                        if (!string.IsNullOrWhiteSpace(dayDto.endTime))
                        {
                            if (!TimeSpan.TryParse(
                                dayDto.endTime,
                                CultureInfo.InvariantCulture,
                                out var parsedEnd))
                            {
                                throw new Exception(
                                    $"Giờ kết thúc của {dayName} không hợp lệ.");
                            }

                            endTime = parsedEnd;
                        }

                        if (startTime.HasValue && endTime.HasValue
                            && startTime.Value >= endTime.Value)
                        {
                            throw new Exception(
                                $"Giờ bắt đầu phải nhỏ hơn giờ kết thúc ở {dayName}.");
                        }

                        var schedule = new ServiceSchedule
                        {
                            ServiceId = service.Id,
                            DayOfWeek = dayName,
                            Enabled = dayDto.enabled,
                            StartTime = startTime,
                            EndTime = endTime
                        };

                        // Liên kết các ca làm việc qua bảng ServiceScheduleShift
                        if (dayDto.shiftIds != null)
                        {
                            foreach (var shiftCode in dayDto.shiftIds.Distinct())
                            {
                                schedule.Shifts.Add(new ServiceScheduleShift
                                {
                                    SheduleActiveId = shiftMap[shiftCode]
                                });
                            }
                        }

                        _context.Set<ServiceSchedule>().Add(schedule);
                    }
                }

                await _context.SaveChangesAsync();

                // 9. Đọc lại dữ liệu thực tế từ database
                var savedService = await _context.Services
                    .AsNoTracking()
                    .Include(s => s.Counters)
                    .Include(s => s.Schedules)
                        .ThenInclude(s => s.Shifts)
                    .FirstOrDefaultAsync(s => s.Id == service.Id);

                if (savedService == null)
                    throw new Exception("Không tìm thấy dịch vụ sau khi tạo.");

                // 10. Map dữ liệu đã lưu sang Response DTO
                var response = new ServiceResponseDto
                {
                    id = savedService.Id,
                    serviceCode = savedService.ServiceId,
                    serviceName = savedService.ServiceName,

                    counterIds = savedService.Counters
                        .Select(c => c.CounterId)
                        .ToList(),

                    status = savedService.Status,
                    createdAt = savedService.CreatedAt,
                    description = savedService.Description,
                    assignmentMethod = savedService.AssignmentMethod,

                    schedule = savedService.Schedules.ToDictionary(
                        s => s.DayOfWeek,
                        s => new ServiceScheduleDayDto
                        {
                            enabled = s.Enabled,
                            startTime = s.StartTime?.ToString(@"hh\:mm"),
                            endTime = s.EndTime?.ToString(@"hh\:mm"),
                            shiftIds = s.Shifts
                                .Select(sh => sh.SheduleActiveId.ToString())
                                .ToList()
                        }),

                    imageUrl = savedService.Image
                };

                await transaction.CommitAsync();

                return response;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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
        public async Task<(bool Success, string Message)> DeleteServiceAsync(DeleteServiceDto dto)
        {
            foreach (var serviceId in dto.ServiceIds)
            {
                var result = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
                 if(result == null)
                return (false, $"Không tìm thấy dịch vụ có ID = {serviceId}");

                bool hasTickets = await _context.Tickets.AnyAsync(t => t.ServiceId == result.Id);
                bool hasBookings = await _context.Bookings.AnyAsync(b => b.ServiceId == result.Id);

                if (hasTickets || hasBookings)
                    return (false, "Không thể xóa dịch vụ này vì đã có phiếu STT hoặc lịch hẹn phát sinh. Hãy chuyển trạng thái sang 'Ngưng hoạt động'.");

                _context.Services.Remove(result);
            }
            await _context.SaveChangesAsync();

            return (true, "Xóa dịch vụ thành công.");
        }

        
    }
}