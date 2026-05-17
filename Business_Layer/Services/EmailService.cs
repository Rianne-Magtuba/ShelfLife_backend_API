using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business_Layer.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Business_Layer.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendResetPasswordEmail(
            string recipientEmail,
            string resetLink)
        {
            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            email.To.Add(MailboxAddress.Parse(recipientEmail));

            email.Subject = "Reset Your ShelfLife Password";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
                    <h2>Reset Password</h2>
                    <p>Click the button below to reset your password:</p>

                    <a href='{resetLink}'
                       style='
                           background:#4CAF50;
                           color:white;
                           padding:12px 20px;
                           text-decoration:none;
                           border-radius:5px;
                       '>
                       Reset Password
                    </a>

                    <p>If you did not request this, ignore this email.</p>
                "
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.SmtpServer,
                _settings.Port,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _settings.SenderEmail,
                _settings.AppPassword);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
