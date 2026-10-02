using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillsSystem.Infrastructure.Email
{
    // الإرسال الفعلي. بيشتغل مع أي SMTP (Mailtrap / Gmail / SendGrid ...) والتغيير من appsettings بس
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Email is disabled; skipped '{Subject}' to {To}", subject, toEmail);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                _logger.LogWarning("Email is enabled but Email:Host / Email:FromEmail are not configured");
                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName, Encoding.UTF8),
                    Subject = subject,
                    SubjectEncoding = Encoding.UTF8,
                    Body = htmlBody,
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = true
                };
                message.To.Add(new MailAddress(toEmail, toName, Encoding.UTF8));

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                if (!string.IsNullOrWhiteSpace(_settings.UserName))
                    client.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);

                // SmtpClient.Timeout مش بيأثر على SendMailAsync، فبنستخدم Token بمهلة
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, _settings.TimeoutSeconds)));

                await client.SendMailAsync(message, timeout.Token);

                _logger.LogInformation("Email '{Subject}' sent to {To}", subject, toEmail);
                return true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;   // التطبيق بيقفل
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email '{Subject}' to {To}", subject, toEmail);
                return false;
            }
        }
    }
}
