using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.ViewModels;

namespace SystemQMS.Services
{
    public class RoleService : IRoleService
    {
        private readonly MyDbContext _context;

        public RoleService(MyDbContext context)
        {
            _context = context;
        }

        private static List<string> ParsePermissions(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
            }
            catch
            {
                return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        private static string SerializePermissions(List<string>? permissions)
        {
            var clean = (permissions ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToList();

            return JsonSerializer.Serialize(clean);
        }

        public async Task<PageResultDto<RoleDto>> GetRolesAsync(int page, int pageSize, string? keyword)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);

            var query = _context.Roles
                .Include(r => r.Users)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(r =>
                    (r.RoleName != null && r.RoleName.ToLower().Contains(kw)) ||
                    (r.Description != null && r.Description.ToLower().Contains(kw)));
            }

            var total = await query.CountAsync();

            var rawList = await query
                .OrderByDescending(r => r.RoleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rawList.Select(r => new RoleDto
            {
                RoleId = r.RoleId.ToString(),
                RoleName = r.RoleName ?? string.Empty,
                Description = r.Description ?? string.Empty,
                UserCount = r.Users.Count,
                Permissions = ParsePermissions(r.Permissions)
            }).ToList();

            return new PageResultDto<RoleDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int roleId)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null) return null;

            return new RoleDto
            {
                RoleId = role.RoleId.ToString(),
                RoleName = role.RoleName ?? string.Empty,
                Description = role.Description ?? string.Empty,
                UserCount = role.Users.Count,
                Permissions = ParsePermissions(role.Permissions)
            };
        }

        public List<PermissionGroupDto> GetPermissionGroups()
        {
            static List<PermissionFunctionDto> Crud(string prefix) => new()
            {
                new() { Code = $"{prefix}_VIEW", Name = "Xem" },
                new() { Code = $"{prefix}_CREATE", Name = "Thêm" },
                new() { Code = $"{prefix}_UPDATE", Name = "Sửa" },
                new() { Code = $"{prefix}_DELETE", Name = "Xóa" },
            };

            return new List<PermissionGroupDto>
            {
                new()
                {
                    GroupCode = "DASHBOARD",
                    GroupName = "Dashboard",
                    Functions = new() { new() { Code = "DASHBOARD_VIEW", Name = "Xem" } }
                },
                new()
                {
                    GroupCode = "DEVICE",
                    GroupName = "Thiết bị",
                    Functions = Crud("DEVICE")
                },
                new()
                {
                    GroupCode = "SERVICE",
                    GroupName = "Dịch vụ",
                    Functions = Crud("SERVICE")
                },
                new()
                {
                    GroupCode = "COUNTER",
                    GroupName = "Quầy dịch vụ",
                    Functions = new List<PermissionFunctionDto>(Crud("COUNTER"))
                    {
                        new() { Code = "COUNTER_OPERATE", Name = "Xử lý quầy" }
                    }
                },
                new()
                {
                    GroupCode = "TICKET",
                    GroupName = "Cấp số",
                    Functions = Crud("TICKET")
                },
                new()
                {
                    GroupCode = "RATING",
                    GroupName = "Đánh giá",
                    Functions = new() { new() { Code = "RATING_VIEW", Name = "Xem" } }
                },
                new()
                {
                    GroupCode = "SETTINGS",
                    GroupName = "Cài đặt hệ thống",
                    Functions = Crud("SETTINGS")
                }
            };
        }

        public async Task<RoleDto> CreateRoleAsync(CreateUpdateRoleDto dto)
        {
            var normalizedName = dto.RoleName.Trim();

            var exists = await _context.Roles
                .AnyAsync(r => r.RoleName != null && r.RoleName.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                throw new InvalidOperationException("Tên vai trò đã tồn tại trong hệ thống.");
            }

            var role = new Roles
            {
                RoleName = normalizedName,
                Description = dto.Description?.Trim() ?? string.Empty,
                Permissions = SerializePermissions(dto.Permissions)
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = role.RoleId.ToString(),
                RoleName = role.RoleName ?? string.Empty,
                Description = role.Description ?? string.Empty,
                UserCount = 0,
                Permissions = ParsePermissions(role.Permissions)
            };
        }

        public async Task<RoleDto?> UpdateRoleAsync(int roleId, CreateUpdateRoleDto dto)
        {
            var role = await _context.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role == null) return null;

            var normalizedName = dto.RoleName.Trim();

            var duplicate = await _context.Roles
                .AnyAsync(r => r.RoleId != roleId
                            && r.RoleName != null
                            && r.RoleName.ToLower() == normalizedName.ToLower());

            if (duplicate)
            {
                throw new InvalidOperationException("Tên vai trò đã tồn tại trong hệ thống.");
            }

            role.RoleName = normalizedName;
            role.Description = dto.Description?.Trim() ?? string.Empty;
            role.Permissions = SerializePermissions(dto.Permissions);

            await _context.SaveChangesAsync();

            return new RoleDto
            {
                RoleId = role.RoleId.ToString(),
                RoleName = role.RoleName ?? string.Empty,
                Description = role.Description ?? string.Empty,
                UserCount = role.Users.Count,
                Permissions = ParsePermissions(role.Permissions)
            };
        }

        public async Task DeleteRolesAsync(List<int> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
            {
                throw new InvalidOperationException("Vui lòng chọn ít nhất một vai trò để xóa.");
            }

            var roles = await _context.Roles
                .Include(r => r.Users)
                .Where(r => roleIds.Contains(r.RoleId))
                .ToListAsync();

            if (roles.Count == 0)
            {
                throw new InvalidOperationException("Không tìm thấy vai trò cần xóa.");
            }

            var inUseRoles = roles
                .Where(r => r.Users.Any())
                .Select(r => r.RoleName ?? $"ID {r.RoleId}")
                .ToList();

            if (inUseRoles.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Không thể xóa vai trò đang có người dùng: {string.Join(", ", inUseRoles)}.");
            }

            _context.Roles.RemoveRange(roles);
            await _context.SaveChangesAsync();
        }
    }
}