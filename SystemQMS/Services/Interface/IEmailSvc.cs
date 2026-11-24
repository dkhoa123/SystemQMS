namespace SystemQMS.Services.Interface
{
    public interface IEmailSvc
    {
        Task SendEmailAsync(
        string to,
        string subject,
        string body
    );
    }
}
