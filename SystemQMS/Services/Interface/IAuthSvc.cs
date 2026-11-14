using SystemQMS.Models;
using SystemQMS.ViewModels.AuthDto;

namespace SystemQMS.Services.Interface
{
    public interface IAuthSvc
    {
        public RegisterDto Register(RegisterDto users);
        public ResponceToken Login(LoginDto users);

        Task ForgotPasswordAsync(ForgotPasswordDto dto);

        Task<bool> VerifyOtpAsync(VerifyOtpDto dto);

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    }
}
