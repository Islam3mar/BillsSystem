using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.Interfaces
{
    // العقد اللي الـ AuthService بيتعامل معاه بس - مش عارف بيانات الأدمن جاية منين
    public interface IAdminCredentialsProvider
    {
        Task<(string Username, string PasswordHash)?> GetAdminCredentialsAsync();
    }
}
