using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BillsSystem.Infrastructure.Security
{
    // التنفيذ الحالي: بيقرا من appsettings.json
    // لما نتوسع لنظام Users/Roles، هنعمل تنفيذ جديد (DatabaseAdminCredentialsProvider)
    // ونبدله في InfrastructureServicesRegistrations.cs بس، من غير ما نلمس AuthService أو الـ Controller خالص
    public class AppSettingsAdminCredentialsProvider : IAdminCredentialsProvider
    {
        private readonly IConfiguration _configuration;

        public AppSettingsAdminCredentialsProvider(IConfiguration configuration) => _configuration = configuration;

        public Task<(string Username, string PasswordHash)?> GetAdminCredentialsAsync()
        {
            var username = _configuration["AdminCredentials:Username"];
            var passwordHash = _configuration["AdminCredentials:PasswordHash"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHash))
                return Task.FromResult<(string, string)?>(null);

            return Task.FromResult<(string, string)?>((username, passwordHash));
        }
    }
}
