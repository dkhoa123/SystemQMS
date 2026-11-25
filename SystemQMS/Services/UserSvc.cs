using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SystemQMS.Models;
using SystemQMS.Services.Interface;
using SystemQMS.ViewModels;
using SystemQMS.ViewModels.UserDto;

namespace SystemQMS.Services
{
    public class UserSvc : IUserSvc
    {
        private readonly MyDbContext _context;
        private readonly IPasswordHasher<Users> _passwordHasher;
        public UserSvc(MyDbContext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public string DeleteUser(int id)
        {
            var userToDelete = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (userToDelete == null)
            {
                throw new Exception("User not found");
            }
            _context.Users.Remove(userToDelete);
            _context.SaveChanges();
            return "User deleted successfully";
        }

        public List<Users> GetAllUsers()
        {
            var users = _context.Users.ToList();

            return users;
        }

        public Users GetUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }

        public Users UpdateUser(int id, Users user)
        {
            var userToUpdate = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (userToUpdate == null)
            {
                throw new Exception("User not found");
            }

            userToUpdate.Name = user.Name;
            userToUpdate.Username = user.Username;
            userToUpdate.Email = user.Email;
            userToUpdate.NumberPhone = user.NumberPhone;
            userToUpdate.RoleId = user.RoleId;
            
            return userToUpdate;
        }

        // Thêm mới các hàm cho quản lý tài khoản 

        public async Task<PageResultDto<UserItemDto>> GetPagedUsersAsync(UserQueryDto query)
        {
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            var usersQuery = _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                string kw = query.Keyword.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.Username != null && u.Username.ToLower().Contains(kw)) ||
                    (u.Name != null && u.Name.ToLower().Contains(kw)) ||
                    (u.Email != null && u.Email.ToLower().Contains(kw)) ||
                    (u.NumberPhone != null && u.NumberPhone.ToLower().Contains(kw))
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Status) &&
                !query.Status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (query.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = usersQuery.Where(u => u.IsActive);
                }
                else if (query.Status.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase) ||
                         query.Status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    usersQuery = usersQuery.Where(u => !u.IsActive);
                }
            }

            if (query.RoleId.HasValue && query.RoleId.Value > 0)
            {
                usersQuery = usersQuery.Where(u => u.RoleId == query.RoleId.Value);
            }

            int total = await usersQuery.CountAsync();

            var items = await usersQuery
                .OrderByDescending(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserItemDto
                {
                    UserId = u.UserId,
                    FullName = u.Name ?? string.Empty,
                    Username = u.Username ?? string.Empty,
                    Email = u.Email,
                    Phone = u.NumberPhone,
                    RoleId = u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName ?? string.Empty : string.Empty,
                    Status = u.IsActive ? "ACTIVE" : "SUSPENDED",
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return new PageResultDto<UserItemDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UserItemDto> GetUserDetailAsync(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");
            }

            return MapToDto(user, user.Role?.RoleName);
        }

        public async Task<UserItemDto> CreateUserAsync(CreateUserDto dto)
        {
            string fullName = (dto.FullName ?? string.Empty).Trim();
            string username = (dto.Username ?? string.Empty).Trim();
            string? email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            string? phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("Vui lòng nhập họ tên.");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Vui lòng nhập tên đăng nhập.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new InvalidOperationException("Vui lòng nhập mật khẩu.");

            if (!string.IsNullOrEmpty(dto.ConfirmPassword) && dto.Password != dto.ConfirmPassword)
                throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

            if (phone != null && phone.Length > 10)
                throw new InvalidOperationException("Số điện thoại không được vượt quá 10 ký tự.");

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username != null && u.Username.ToLower() == username.ToLower());
            if (usernameExists)
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.");

            if (email != null)
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower());
                if (emailExists)
                    throw new InvalidOperationException("Email đã được sử dụng cho tài khoản khác.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId);
            if (role == null)
                throw new InvalidOperationException("Vai trò được chọn không tồn tại.");

            // Lấy UnitId & CounterId hợp lệ trong DB 
            int defaultUnitId = await _context.Units.AnyAsync(u => u.UnitId == 1)
                ? 1
                : await _context.Units.Select(u => u.UnitId).FirstOrDefaultAsync();

            // int defaultCounterId = await _context.Counters.AnyAsync(c => c.CounterId == 1)
            //     ? 1
            //     : await _context.Counters.Select(c => c.CounterId).FirstOrDefaultAsync();
            int defaultCounterId = 1;

            var newUser = new Users
            {
                Name = fullName,
                Username = username,
                Email = email,
                NumberPhone = phone,
                RoleId = dto.RoleId,
                IsActive = ParseIsActive(dto.Status, true),
                UnitId = defaultUnitId,
                CounterId = defaultCounterId
            };

            newUser.Password = _passwordHasher.HashPassword(newUser, dto.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return MapToDto(newUser, role.RoleName);
        }

        public async Task<UserItemDto> UpdateUserAccountAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            string fullName = (dto.FullName ?? string.Empty).Trim();
            string username = (dto.Username ?? string.Empty).Trim();
            string? email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            string? phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("Vui lòng nhập họ tên.");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Vui lòng nhập tên đăng nhập.");

            if (phone != null && phone.Length > 10)
                throw new InvalidOperationException("Số điện thoại không được vượt quá 10 ký tự.");

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.UserId != id && u.Username != null && u.Username.ToLower() == username.ToLower());
            if (usernameExists)
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại trong hệ thống.");

            if (email != null)
            {
                bool emailExists = await _context.Users
                    .AnyAsync(u => u.UserId != id && u.Email != null && u.Email.ToLower() == email.ToLower());
                if (emailExists)
                    throw new InvalidOperationException("Email đã được sử dụng cho tài khoản khác.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == dto.RoleId);
            if (role == null)
                throw new InvalidOperationException("Vai trò được chọn không tồn tại.");

            user.Name = fullName;
            user.Username = username;
            user.Email = email;
            user.NumberPhone = phone;
            user.RoleId = dto.RoleId;
            user.IsActive = ParseIsActive(dto.Status, user.IsActive);

            await _context.SaveChangesAsync();

            return MapToDto(user, role.RoleName);
        }

        public async Task ChangePasswordAsync(int id, ChangeUserPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy tài khoản.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new InvalidOperationException("Vui lòng nhập mật khẩu mới.");

            if (!string.IsNullOrEmpty(dto.ConfirmPassword) && dto.NewPassword != dto.ConfirmPassword)
                throw new InvalidOperationException("Mật khẩu xác nhận không khớp.");

            user.Password = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUsersAsync(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                throw new InvalidOperationException("Vui lòng chọn ít nhất một tài khoản để xóa.");

            var distinctIds = ids.Distinct().ToList();
            var users = await _context.Users
                .Where(u => distinctIds.Contains(u.UserId))
                .ToListAsync();

            if (users.Count == 0)
                throw new KeyNotFoundException("Không tìm thấy tài khoản cần xóa.");

            foreach (var user in users)
            {
                bool hasHistory =
                    await _context.Tickets.AnyAsync(t => t.UserId == user.UserId) ||
                    await _context.Diaries.AnyAsync(d => d.UserId == user.UserId) ||
                    await _context.News.AnyAsync(n => n.CreatedById == user.UserId || n.UpdatedById == user.UserId);

                if (hasHistory)
                {
                    throw new InvalidOperationException(
                        $"Không thể xóa tài khoản '{user.Username}' vì đã phát sinh dữ liệu lịch sử hoặc cấp số liên quan."
                    );
                }
            }

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
        }

        private static bool ParseIsActive(string? status, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(status))
                return defaultValue;

            if (status.Equals("SUSPENDED", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase))
                return false;

            if (status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase))
                return true;

            return defaultValue;
        }

        private static UserItemDto MapToDto(Users user, string? roleName)
        {
            return new UserItemDto
            {
                UserId = user.UserId,
                FullName = user.Name ?? string.Empty,
                Username = user.Username ?? string.Empty,
                Email = user.Email,
                Phone = user.NumberPhone,
                RoleId = user.RoleId,
                RoleName = roleName ?? string.Empty,
                Status = user.IsActive ? "ACTIVE" : "SUSPENDED",
                IsActive = user.IsActive
            };
        }
    }
}
