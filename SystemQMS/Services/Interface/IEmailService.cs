namespace SystemQMS.Services.Interface
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp);
    }
}
