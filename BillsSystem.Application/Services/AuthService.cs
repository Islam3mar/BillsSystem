using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.Common;
using BillsSystem.Application.Interfaces;

namespace BillsSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAdminCredentialsProvider _credentialsProvider;

        public AuthService(IAdminCredentialsProvider credentialsProvider) => _credentialsProvider = credentialsProvider;

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var credentials = await _credentialsProvider.GetAdminCredentialsAsync();
            if (credentials == null) return false;

            var (storedUsername, storedHash) = credentials.Value;

            // بنفحص الباسورد دايمًا (حتى لو اسم المستخدم غلط) عشان زمن الرد ميبيّنش هل اسم المستخدم صح
            var usernameMatches = string.Equals(username?.Trim(), storedUsername, StringComparison.OrdinalIgnoreCase);
            var passwordMatches = PasswordHasher.Verify(password ?? string.Empty, storedHash);

            return usernameMatches && passwordMatches;
        }
    }
}
