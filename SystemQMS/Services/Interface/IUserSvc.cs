using SystemQMS.Models;
using SystemQMS.ViewModels;
using SystemQMS.ViewModels.UserDto;

namespace SystemQMS.Services.Interface
{
    public interface IUserSvc
    {
        public Users GetUser(int id);
        public Users UpdateUser(int id, Users user);
        public List<Users> GetAllUsers();

        public string DeleteUser(int id);

        // Thêm mới các hàm mới cho Quản lý tài khoản 
        Task<PageResultDto<UserItemDto>> GetPagedUsersAsync(UserQueryDto query);
        Task<UserItemDto> GetUserDetailAsync(int id);
        Task<UserItemDto> CreateUserAsync(CreateUserDto dto);
        Task<UserItemDto> UpdateUserAccountAsync(int id, UpdateUserDto dto);
        Task ChangePasswordAsync(int id, ChangeUserPasswordDto dto);
        Task DeleteUsersAsync(List<int> ids);

    }
}
