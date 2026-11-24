
using SystemQMS.Models;

namespace SystemQMS.Services.Interface
{
    public interface IUserSvc
    {
        public Users GetUser(int id);
        public Users UpdateUser(int id, Users user);
        public List<Users> GetAllUsers();

        public string DeleteUser(int id);

    }
}
