using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Application.Interfaces;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Reports;
using Microsoft.Extensions.Options;

namespace BillsSystem.Application.Services
{
    // قوالب إيميلات العملاء (عربي RTL). عايز تغيّر النص؟ عدّل هنا بس
    public class ClientEmailService : IClientEmailService
    {
        private readonly IEmailService _email;
        private readonly EmailSettings _emailSettings;
        private readonly ReminderSettings _reminderSettings;

        public ClientEmailService(IEmailService email, IOptions<EmailSettings> emailSettings, IOptions<ReminderSettings> reminderSettings)
        {
            _email = email;
            _emailSettings = emailSettings.Value;
            _reminderSettings = reminderSettings.Value;
        }

        public bool IsEnabled => _emailSettings.Enabled;

        // ---------------- تذكير الفاتورة (قبل الاستحقاق / متأخرة) ----------------
        public Task<bool> SendReminderAsync(ReminderCandidateRow bill, ReminderType type, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(bill.ClientEmail)) return Task.FromResult(false);

            var today = AppClock.Today;
            string subject, headline, intro, color;

            if (type == ReminderType.Overdue)
            {
                var late = Math.Max(1, (today - bill.DueDate.Date).Days);
                subject = $"تنبيه: الفاتورة رقم {bill.BillId} متأخرة عن موعد السداد";
                headline = "فاتورتك متأخرة عن موعد السداد";
                intro = $"نود تذكيرك بأن الفاتورة التالية تجاوزت موعد استحقاقها بـ {late} يوم، ولا يزال عليها مبلغ مستحق السداد.";
                color = "#c0392b";
            }
            else
            {
                var left = (bill.DueDate.Date - today).Days;
                var when = left <= 0 ? "اليوم" : left == 1 ? "غدًا" : $"بعد {left} أيام";
                subject = $"تذكير: الفاتورة رقم {bill.BillId} تستحق السداد {when}";
                headline = "تذكير بموعد سداد فاتورة";
                intro = $"نود تذكيرك بأن موعد سداد الفاتورة التالية يحين {when}، ولا يزال عليها مبلغ مستحق.";
                color = "#e67e22";
            }

            var rows = new List<(string Label, string Value)>
            {
                ("رقم الفاتورة", $"#{bill.BillId}"),
                ("تاريخ الفاتورة", Date(bill.BillDate)),
                ("صافي الفاتورة", Money(bill.TheNet)),
                ("المبلغ المتبقي", Money(bill.TheRest)),
                ("تاريخ الاستحقاق", Date(bill.DueDate))
            };

            var html = BuildHtml(bill.ClientName, headline, intro, color, rows,
                "برجاء سداد المبلغ المتبقي في أقرب وقت ممكن. وإذا كنت قد سددت بالفعل فبرجاء تجاهل هذه الرسالة، ونشكرك على تعاونك.");

            return _email.SendAsync(bill.ClientEmail, bill.ClientName, subject, html, ct);
        }

        // ---------------- سقف الدين ----------------
        public Task<bool> SendCreditLimitAsync(string toEmail, string clientName, decimal limit, decimal currentDebt, CancellationToken ct = default)
        {
            var available = Math.Max(0m, limit - currentDebt);

            var subject = "تنبيه: مديونيتك وصلت للحد الأقصى المسموح به";
            var headline = "وصلت للحد الأقصى للمديونية";
            var intro = available <= 0m
                ? "وصلت مديونيتك الحالية إلى الحد الأقصى المسموح به، ولن نتمكن من إصدار فواتير آجلة جديدة لك إلا بعد سداد جزء من المبلغ المستحق."
                : "اقتربت مديونيتك الحالية من الحد الأقصى المسموح به، وقد لا نتمكن من إصدار فواتير آجلة جديدة لك إلا بعد سداد جزء من المبلغ المستحق.";

            var rows = new List<(string Label, string Value)>
            {
                ("الحد الأقصى المسموح به", Money(limit)),
                ("مديونيتك الحالية", Money(currentDebt)),
                ("المتاح حاليًا", Money(available))
            };

            var html = BuildHtml(clientName, headline, intro, "#c0392b", rows,
                "برجاء التواصل معنا لترتيب سداد جزء من المبلغ المستحق. ونشكرك على تعاونك.");

            return _email.SendAsync(toEmail, clientName, subject, html, ct);
        }

        // ---------------- مساعدات ----------------
        private string Money(decimal amount)
            => $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {_reminderSettings.Currency}";

        private static string Date(DateTime date) => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

        private static string H(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

        // Inline CSS بس (أغلب برامج الإيميل بتتجاهل الـ <style>)
        private string BuildHtml(string clientName, string headline, string intro, string color,
            IEnumerable<(string Label, string Value)> rows, string footerNote)
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"ar\" dir=\"rtl\"><head><meta charset=\"utf-8\">");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\"></head>");
            sb.Append("<body style=\"margin:0;padding:24px;background:#f4f6f8;font-family:Tahoma,Arial,sans-serif;color:#222;direction:rtl;text-align:right;\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\"><tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" width=\"560\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width:560px;width:100%;background:#ffffff;border:1px solid #e3e7eb;border-radius:8px;\">");

            sb.Append($"<tr><td style=\"background:{color};color:#ffffff;padding:18px 24px;font-size:18px;font-weight:bold;border-radius:8px 8px 0 0;\">{H(headline)}</td></tr>");

            sb.Append("<tr><td style=\"padding:24px;font-size:15px;line-height:1.8;\">");
            sb.Append($"<p style=\"margin:0 0 12px;\">السيد/ة {H(clientName)}،</p>");
            sb.Append($"<p style=\"margin:0 0 16px;\">{H(intro)}</p>");

            sb.Append("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin:0 0 16px;border:1px solid #eef1f4;\">");
            foreach (var (label, value) in rows)
            {
                sb.Append("<tr>");
                sb.Append($"<td style=\"padding:8px 12px;border-bottom:1px solid #eef1f4;color:#667788;width:42%;\">{H(label)}</td>");
                sb.Append($"<td style=\"padding:8px 12px;border-bottom:1px solid #eef1f4;font-weight:bold;\">{H(value)}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            sb.Append($"<p style=\"margin:0;color:#444;\">{H(footerNote)}</p>");
            sb.Append("</td></tr>");

            sb.Append($"<tr><td style=\"padding:14px 24px;background:#fafbfc;color:#7a8794;font-size:12px;border-top:1px solid #e3e7eb;border-radius:0 0 8px 8px;\">{H(_emailSettings.FromName)}</td></tr>");
            sb.Append("</table></td></tr></table></body></html>");

            return sb.ToString();
        }
    }
}
