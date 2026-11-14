using SystemQMS.Models;
using SystemQMS.ViewModels.AuthDto;

namespace SystemQMS.Services.Interface
{
    public interface IAuthSvc
    {
        public RegisterDto Register(RegisterDto users);
    }
}
