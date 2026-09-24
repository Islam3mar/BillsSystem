using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;

namespace BillsSystem.Application.Common
{
    public static class PasswordHasher
    {
        public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public static bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
