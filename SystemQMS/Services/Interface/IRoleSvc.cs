using SystemQMS.ViewModels.RoleDto;

namespace SystemQMS.Services.Interface
{
    public interface IRoleSvc
    {
        List<PermissionGroupDto> GetPermissionGroups();
        Task<RolePageResultDto<RoleDto>> GetRolesAsync(RoleQueryFilterDto filter);
        Task<RoleDto?> GetRoleByIdAsync(string id);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequestDto request);
        Task<RoleDto> UpdateRoleAsync(string id, UpdateRoleRequestDto request);
        Task DeleteRolesAsync(List<string> ids);
    }
}