using SystemQMS.ViewModels;

namespace SystemQMS.Services
{
    public interface IRoleService
    {
        Task<PageResultDto<RoleDto>> GetRolesAsync(int page, int pageSize, string? keyword);
        Task<RoleDto?> GetRoleByIdAsync(int roleId);
        List<PermissionGroupDto> GetPermissionGroups();
        Task<RoleDto> CreateRoleAsync(CreateUpdateRoleDto dto);
        Task<RoleDto?> UpdateRoleAsync(int roleId, CreateUpdateRoleDto dto);
        Task DeleteRolesAsync(List<int> roleIds);
    }
}