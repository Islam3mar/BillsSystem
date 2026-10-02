using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.Interfaces
{
    public interface IEmailService
    {
        // true = اتبعت. أي فشل (SMTP، إيميل غلط، Timeout) بيرجّع false ويتسجل في الـ Log من غير ما يرمي Exception
        Task<bool> SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default);
    }
}
