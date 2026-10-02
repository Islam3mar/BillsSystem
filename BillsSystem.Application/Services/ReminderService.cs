using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Domain.Reports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BillsSystem.Application.Services
{
    public class ReminderService : IReminderService
    {
        // لو الإرسال فشل كده ورا بعض (غالبًا إعدادات SMTP غلط) نوقف الدورة بدل ما نضرب كل الفواتير
        private const int MaxConsecutiveFailures = 3;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClientEmailService _clientEmails;
        private readonly INotificationService _notifications;
        private readonly ReminderSettings _settings;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IUnitOfWork unitOfWork, IClientEmailService clientEmails,
            INotificationService notifications, IOptions<ReminderSettings> settings, ILogger<ReminderService> logger)
        {
            _unitOfWork = unitOfWork;
            _clientEmails = clientEmails;
            _notifications = notifications;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ReminderRunResult> ProcessDueRemindersAsync(CancellationToken ct = default)
        {
            var result = new ReminderRunResult();
            if (!_settings.Enabled) return result;

            var today = AppClock.Today;
            var candidates = await _unitOfWork.Bills.GetReminderCandidatesAsync(today, _settings.DaysBeforeDue);
            var consecutiveFailures = 0;

            foreach (var bill in candidates)
            {
                ct.ThrowIfCancellationRequested();

                var type = DecideType(bill, today);
                if (type == ReminderType.None) continue;

                result.Due++;
                var hasEmail = !string.IsNullOrWhiteSpace(bill.ClientEmail);

                if (hasEmail)
                {
                    // الإيميل لسه مش متفعّل، أو وصلنا للحد: الفاتورة بتستنى ومش بتتعلّم "اتبعتلها"
                    if (!_clientEmails.IsEnabled ||
                        result.EmailsSent >= _settings.MaxEmailsPerRun ||
                        consecutiveFailures >= MaxConsecutiveFailures)
                    {
                        result.Skipped++;
                        continue;
                    }

                    if (!await _clientEmails.SendReminderAsync(bill, type, ct))
                    {
                        result.Failed++;
                        consecutiveFailures++;
                        continue;   // مش بنعلّمها، فهتتحاول تاني في الدورة الجاية
                    }

                    consecutiveFailures = 0;
                    result.EmailsSent++;
                }
                else
                {
                    result.InternalOnly++;
                }

                await _unitOfWork.Bills.MarkReminderSentAsync(bill.BillId, type, AppClock.Now);
                await _notifications.NotifyAsync(NotificationType.Bill, NotificationAction.Alert,
                    BuildNotificationMessage(bill, type, today, emailed: hasEmail), bill.BillId);
            }

            return result;
        }

        public async Task<(bool Success, string? Error)> SendNowAsync(int billId)
        {
            var bill = await _unitOfWork.Bills.GetReminderCandidateAsync(billId);
            if (bill == null) return (false, "This bill has no due date (or it no longer exists)");
            if (bill.TheRest <= 0) return (false, "This bill is already fully paid");
            if (string.IsNullOrWhiteSpace(bill.ClientEmail)) return (false, "This client has no email address. Add one from the Clients page first");
            if (!_clientEmails.IsEnabled) return (false, "Email sending is turned off (set Email:Enabled to true in appsettings)");

            var today = AppClock.Today;
            var type = bill.DueDate.Date >= today ? ReminderType.BeforeDue : ReminderType.Overdue;

            if (!await _clientEmails.SendReminderAsync(bill, type))
                return (false, "Couldn't send the email. Check the SMTP settings and the application log");

            await _unitOfWork.Bills.MarkReminderSentAsync(bill.BillId, type, AppClock.Now);
            await _notifications.NotifyAsync(NotificationType.Bill, NotificationAction.Alert,
                BuildNotificationMessage(bill, type, today, emailed: true) + " (manual)", bill.BillId);

            _logger.LogInformation("Manual reminder sent for bill {BillId}", billId);
            return (true, null);
        }

        // ---------- القاعدة: امتى نبعت إيه ----------
        // - لسه مفيش تذكير + فاضل DaysBeforeDue يوم أو أقل (أو يوم الاستحقاق) → "قبل الاستحقاق" مرة واحدة
        // - بعد الاستحقاق: أول مرة فورًا، وبعدها كل OverdueRepeatEveryDays يوم لحد ما يسدد
        // لو التطبيق كان مقفول وفات الميعاد: أول ما يشتغل يبعت اللي مناسب للحالة الحالية (Catch-up)
        private ReminderType DecideType(ReminderCandidateRow bill, DateTime today)
        {
            var daysToDue = (bill.DueDate.Date - today).Days;

            if (daysToDue >= 0)
            {
                return daysToDue <= _settings.DaysBeforeDue && bill.LastReminderType == ReminderType.None
                    ? ReminderType.BeforeDue
                    : ReminderType.None;
            }

            if (bill.LastReminderType != ReminderType.Overdue || bill.LastReminderSentAt is null)
                return ReminderType.Overdue;

            var daysSinceLast = (today - bill.LastReminderSentAt.Value.Date).Days;
            return daysSinceLast >= Math.Max(1, _settings.OverdueRepeatEveryDays)
                ? ReminderType.Overdue
                : ReminderType.None;
        }

        private static string BuildNotificationMessage(ReminderCandidateRow bill, ReminderType type, DateTime today, bool emailed)
        {
            var status = type == ReminderType.Overdue
                ? $"overdue by {Math.Max(1, (today - bill.DueDate.Date).Days)} day(s)"
                : $"due on {bill.DueDate:yyyy/MM/dd}";

            var tail = emailed
                ? "Reminder email sent to the client"
                : "Client has no email - contact them manually";

            return $"Bill #{bill.BillId} for '{bill.ClientName}' is {status}. Rest: {bill.TheRest:0.00}. {tail}";
        }
    }
}
