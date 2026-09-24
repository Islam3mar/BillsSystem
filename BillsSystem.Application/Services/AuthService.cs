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

            if (!string.Equals(username?.Trim(), storedUsername, StringComparison.OrdinalIgnoreCase))
                return false;

            return PasswordHasher.Verify(password, storedHash);
        }
    }
}
