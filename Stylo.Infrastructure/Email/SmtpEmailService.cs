using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> options, ILogger<SmtpEmailService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                var socketOptions = _settings.SmtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : (_settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions, cancellationToken);
                await client.AuthenticateAsync(_settings.SmtpUser, _settings.SmtpPassword, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email successfully sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Error: {Message}", toEmail, ex.Message);
            }
        }
    }
}