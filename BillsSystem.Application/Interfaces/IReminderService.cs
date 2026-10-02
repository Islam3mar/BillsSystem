using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;

namespace BillsSystem.Application.Interfaces
{
    public interface IReminderService
    {
        // بيتنادى من الـ Background Service: يفحص كل الفواتير المستحقة ويبعت اللي فاتها ميعادها
        Task<ReminderRunResult> ProcessDueRemindersAsync(CancellationToken ct = default);

        // زرار "Send reminder now" في صفحة الفاتورة
        Task<(bool Success, string? Error)> SendNowAsync(int billId);
    }
}
