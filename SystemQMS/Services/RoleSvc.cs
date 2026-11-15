using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels.RoleDto;

namespace SystemQMS.Services
{
    public class RoleSvc : IRoleSvc
    {
        private readonly MyDbContext _context;

        public RoleSvc(MyDbContext context)
        {
            _context = context;
        }

        public List<PermissionGroupDto> GetPermissionGroups()
        {
            return new List<PermissionGroupDto>
            {
                new()
                {
                    groupCode = "DASHBOARD",
                    groupName = "Dashboard",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "DASHBOARD_VIEW", name = "Xem thống kê Dashboard" },
                        new() { code = "DASHBOARD_EXPORT", name = "Xuất báo cáo số liệu" }
                    }
                },
                new()
                {
                    groupCode = "DEVICE",
                    groupName = "Thiết bị",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "DEVICE_VIEW", name = "Xem danh sách thiết bị" },
                        new() { code = "DEVICE_CREATE", name = "Thêm thiết bị" },
                        new() { code = "DEVICE_UPDATE", name = "Cập nhật thiết bị" },
                        new() { code = "DEVICE_DELETE", name = "Xóa thiết bị" }
                    }
                },
                new()
                {
                    groupCode = "SERVICE",
                    groupName = "Dịch vụ",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "SERVICE_VIEW", name = "Xem danh sách dịch vụ" },
                        new() { code = "SERVICE_CREATE", name = "Thêm dịch vụ" },
                        new() { code = "SERVICE_UPDATE", name = "Cập nhật dịch vụ" },
                        new() { code = "SERVICE_DELETE", name = "Xóa dịch vụ" }
                    }
                },
                new()
                {
                    groupCode = "COUNTER",
                    groupName = "Quầy dịch vụ",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "COUNTER_VIEW", name = "Xem danh sách quầy" },
                        new() { code = "COUNTER_MANAGE", name = "Thêm, sửa, xóa quầy" },
                        new() { code = "COUNTER_OPERATE", name = "Vận hành quầy & gọi số" }
                    }
                },
                new()
                {
                    groupCode = "TICKET",
                    groupName = "Cấp số",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "TICKET_VIEW", name = "Xem danh sách cấp số" },
                        new() { code = "TICKET_CREATE", name = "Cấp số mới" },
                        new() { code = "TICKET_EXPORT", name = "Xuất dữ liệu cấp số" }
                    }
                },
                new()
                {
                    groupCode = "RATING",
                    groupName = "Đánh giá",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "RATING_VIEW", name = "Xem danh sách đánh giá" },
                        new() { code = "RATING_MANAGE", name = "Quản lý mẫu đánh giá" }
                    }
                },
                new()
                {
                    groupCode = "SETTING",
                    groupName = "Cài đặt hệ thống",
                    functions = new List<PermissionFunctionDto>
                    {
                        new() { code = "SETTING_ROLE", name = "Quản lý vai trò" },
                        new() { code = "SETTING_ACCOUNT", name = "Quản lý tài khoản" },
                        new() { code = "SETTING_CONFIG", name = "Cấu hình Logo, giao diện & âm thanh" }
                    }
                }
            };
        }

        public async Task<RolePageResultDto<RoleDto>> GetRolesAsync(RoleQueryFilterDto filter)
        {
            var query = _context.roles
                .Include(r => r.Users)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.keyword))
            {
                var kw = filter.keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.roleName != null && r.roleName.ToLower().Contains(kw)) ||
                    (r.description != null && r.description.ToLower().Contains(kw)));
            }

            var total = await query.CountAsync();
            var page = filter.page <= 0 ? 1 : filter.page;
            var pageSize = filter.pageSize <= 0 ? 10 : filter.pageSize;

            var roles = await query
                .OrderBy(r => r.roleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new RolePageResultDto<RoleDto>
            {
                items = roles.Select(MapToDto).ToList(),
                page = page,
                pageSize = pageSize,
                total = total
            };
        }

        public async Task<RoleDto?> GetRoleByIdAsync(string id)
        {
            if (!int.TryParse(id, out int roleId))
                return null;

            var role = await _context.roles
                .Include(r => r.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.roleId == roleId);

            return role == null ? null : MapToDto(role);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request)
        {
            var normalizedName = request.roleName.Trim();
            bool exists = await _context.roles
                .AnyAsync(r => r.roleName != null && r.roleName.ToLower() == normalizedName.ToLower());

            if (exists)
                throw new InvalidOperationException($"Tên vai trò '{normalizedName}' đã tồn tại");

            var cleanPermissions = NormalizePermissions(request.permissions);

            var newRole = new Roles
            {
                roleName = normalizedName,
                description = request.description?.Trim(),
                permissions = string.Join(",", cleanPermissions)
            };

            _context.roles.Add(newRole);
            await _context.SaveChangesAsync();

            return MapToDto(newRole);
        }

        public async Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleRequestDto request)
        {
            if (!int.TryParse(id, out int roleId))
                throw new KeyNotFoundException("Không tìm thấy vai trò");

            var role = await _context.roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.roleId == roleId);

            if (role == null)
                throw new KeyNotFoundException("Không tìm thấy vai trò");

            var normalizedName = request.roleName.Trim();
            bool duplicate = await _context.roles
                .AnyAsync(r => r.roleId != roleId && r.roleName != null && r.roleName.ToLower() == normalizedName.ToLower());

            if (duplicate)
                throw new InvalidOperationException($"Tên vai trò '{normalizedName}' đã tồn tại");

            role.roleName = normalizedName;
            role.description = request.description?.Trim();
            role.permissions = string.Join(",", NormalizePermissions(request.permissions));

            await _context.SaveChangesAsync();
            return MapToDto(role);
        }

        public async Task DeleteRolesAsync(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new ArgumentException("Vui lòng chọn vai trò cần xóa");

            var roleIds = ids
                .Select(id => int.TryParse(id, out int parsed) ? parsed : -1)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var targets = await _context.roles
                .Include(r => r.Users)
                .Where(r => roleIds.Contains(r.roleId))
                .ToListAsync();

            if (targets.Any(r => r.Users.Any()))
                throw new InvalidOperationException("Không thể xóa vai trò đang có tài khoản được phân quyền");

            _context.roles.RemoveRange(targets);
            await _context.SaveChangesAsync();
        }

        private static List<string> NormalizePermissions(List<string>? permissions)
        {
            if (permissions == null) return new List<string>();
            return permissions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim().ToUpper())
                .Distinct()
                .ToList();
        }

        private static RoleDto MapToDto(Roles role)
        {
            var permissionList = string.IsNullOrWhiteSpace(role.permissions)
                ? new List<string>()
                : role.permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

            return new RoleDto
            {
                roleId = role.roleId.ToString(),
                roleName = role.roleName ?? string.Empty,
                description = role.description ?? string.Empty,
                userCount = role.Users?.Count ?? 0,
                permissions = permissionList
            };
        }
    }
}