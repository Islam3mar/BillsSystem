using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillsSystem.Infrastructure.Background
{
    // Catch-up: بيفحص أول ما التطبيق يشتغل (بعد تأخير بسيط) وبعدها كل CheckIntervalMinutes.
    // مبيعتمدش على ميعاد ثابت، فلو التطبيق كان مقفول أي تذكير فات بيتبعت أول ما يرجع.
    public class BillReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ReminderSettings _settings;
        private readonly ILogger<BillReminderBackgroundService> _logger;

        private DateTime _lastPurgeDate = DateTime.MinValue;   // تنظيف الإشعارات مرة في اليوم بس

        public BillReminderBackgroundService(IServiceScopeFactory scopeFactory,
            IOptions<ReminderSettings> settings, ILogger<BillReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_settings.Enabled)
                _logger.LogInformation("Bill reminders are disabled (Reminders:Enabled = false); only notification cleanup will run");

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(0, _settings.StartupDelaySeconds)), stoppingToken);

                using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _settings.CheckIntervalMinutes)));
                do
                {
                    if (_settings.Enabled) await RunOnceAsync(stoppingToken);
                    await PurgeNotificationsAsync(stoppingToken);
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // التطبيق بيقفل — طبيعي
            }
        }

        private async Task RunOnceAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminders = scope.ServiceProvider.GetRequiredService<IReminderService>();

                var result = await reminders.ProcessDueRemindersAsync(ct);

                _logger.LogInformation(
                    "Reminder run finished: {Due} due, {Sent} emails sent, {Internal} internal-only, {Skipped} skipped, {Failed} failed",
                    result.Due, result.EmailsSent, result.InternalOnly, result.Skipped, result.Failed);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // أي خطأ (الداتابيز مش متاحة مثلًا) متسجل ومبيوقفش الخدمة، والدورة الجاية بتحاول تاني
                _logger.LogError(ex, "Reminder run failed");
            }
        }

        // مرة في اليوم: مسح الإشعارات القديمة. فشله مبيأثرش على التذكيرات
        private async Task PurgeNotificationsAsync(CancellationToken ct)
        {
            if (_lastPurgeDate == AppClock.Today) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var removed = await notifications.PurgeOldAsync(_settings.NotificationRetentionDays);
                _lastPurgeDate = AppClock.Today;

                if (removed > 0)
                    _logger.LogInformation("Purged {Count} notifications older than {Days} days", removed, _settings.NotificationRetentionDays);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification cleanup failed");
            }
        }
    }
}
