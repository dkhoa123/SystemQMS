using System.Net;
using System.Net.Mail;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            string fromEmail = _configuration["EmailSettings:Email"]
                ?? throw new Exception("Thiếu EmailSettings:Email");

            string appPassword = _configuration["EmailSettings:AppPassword"]
                ?? throw new Exception("Thiếu EmailSettings:AppPassword");

            string host = _configuration["EmailSettings:Host"]
                ?? throw new Exception("Thiếu EmailSettings:Host");

            int port = int.Parse(
                _configuration["EmailSettings:Port"]
                ?? throw new Exception("Thiếu EmailSettings:Port"));

            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, "SystemQMS");
            message.To.Add(toEmail);
            message.Subject = "Mã OTP đăng ký SystemQMS";
            message.Body = $"Mã OTP của bạn là: {otp}\nMã có hiệu lực trong 5 phút.";
            message.IsBodyHtml = false;

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            await smtp.SendMailAsync(message);
        }
    }
}