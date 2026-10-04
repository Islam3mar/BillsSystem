using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BillsSystem.Infrastructure.Email
{
    // الإرسال الفعلي بـ MailKit. بيشتغل مع أي SMTP (Mailtrap / Gmail / SendGrid ...) والتغيير من appsettings بس
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
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                var timeoutSeconds = Math.Max(5, _settings.TimeoutSeconds);
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                using var client = new SmtpClient { Timeout = timeoutSeconds * 1000 };

                await client.ConnectAsync(_settings.Host, _settings.Port, ParseSecurity(_settings.Security), timeout.Token);

                if (!string.IsNullOrWhiteSpace(_settings.UserName))
                    await client.AuthenticateAsync(_settings.UserName, _settings.Password, timeout.Token);

                await client.SendAsync(message, timeout.Token);
                await client.DisconnectAsync(true, CancellationToken.None);

                _logger.LogInformation("Email '{Subject}' sent to {To}", subject, toEmail);
                return true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;   // التطبيق بيقفل
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("SMTP timed out after {Seconds}s sending '{Subject}' to {To} via {Host}:{Port}. Check the host/port and the Email:Security setting",
                    _settings.TimeoutSeconds, subject, toEmail, _settings.Host, _settings.Port);
                return false;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "SMTP connection timed out sending '{Subject}' to {To} via {Host}:{Port}", subject, toEmail, _settings.Host, _settings.Port);
                return false;
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogError(ex, "SMTP login was rejected for {User}. Check Email:UserName / Email:Password (Gmail needs an App Password, not the account password)", _settings.UserName);
                return false;
            }
            catch (SslHandshakeException ex)
            {
                _logger.LogError(ex, "TLS handshake failed with {Host}:{Port}. Port and Email:Security probably don't match (587 = StartTls, 465 = SslOnConnect)", _settings.Host, _settings.Port);
                return false;
            }
            catch (SmtpCommandException ex)
            {
                // السيرفر رد برفض واضح: غالبًا إيميل المستلم أو المرسل غلط/مرفوض
                _logger.LogError(ex, "SMTP server rejected '{Subject}' to {To}: {ErrorCode} / {StatusCode}", subject, toEmail, ex.ErrorCode, ex.StatusCode);
                return false;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Couldn't reach {Host}:{Port} ({SocketError}). Check the network/firewall", _settings.Host, _settings.Port, ex.SocketErrorCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email '{Subject}' to {To}", subject, toEmail);
                return false;
            }
        }

        private static SecureSocketOptions ParseSecurity(string? value)
            => Enum.TryParse<SecureSocketOptions>(value, ignoreCase: true, out var option) ? option : SecureSocketOptions.Auto;
    }
}
