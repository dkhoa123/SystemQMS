using SystemQMS.Models;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class UserSvc : IUserSvc
    {
        private readonly MyDbContext _context;
        public UserSvc(MyDbContext context)
        {
            _context = context;
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
    }
}
