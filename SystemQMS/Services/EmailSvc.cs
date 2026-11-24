using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SystemQMS.Services.Interface;

namespace SystemQMS.Services
{
    public class EmailSvc : IEmailSvc
    {
        private readonly IConfiguration _configuration;

        public EmailSvc(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body)
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                "SystemQMS",
                _configuration["SendMailsGoogle:senderMail"]
            ));

            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["SendMailsGoogle:smtpServer"],
                int.Parse(_configuration["SendMailsGoogle:portNum"]),
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                _configuration["SendMailsGoogle:senderMail"],
                _configuration["SendMailsGoogle:passwordSender"]
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
