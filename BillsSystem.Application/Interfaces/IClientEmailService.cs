using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Enums;
using BillsSystem.Domain.Reports;

namespace BillsSystem.Application.Interfaces
{
    // إيميلات العملاء الجاهزة (القوالب هنا، والإرسال الفعلي في IEmailService)
    public interface IClientEmailService
    {
        bool IsEnabled { get; }

        Task<bool> SendReminderAsync(ReminderCandidateRow bill, ReminderType type, CancellationToken ct = default);

        Task<bool> SendCreditLimitAsync(string toEmail, string clientName, decimal limit, decimal currentDebt, CancellationToken ct = default);
    }
}
