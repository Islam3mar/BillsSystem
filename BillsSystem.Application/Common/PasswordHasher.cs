using System;
using System.Collections.Generic;
using System.Text;
using BCrypt.Net;

namespace BillsSystem.Application.Common
{
    public static class PasswordHasher
    {
        public static string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

        // لو الـ Hash في appsettings اتبوّظ (أو الباسورد null) بنرجع false بدل ما نرمي Exception (500)
        public static bool Verify(string password, string hash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch (BCrypt.Net.SaltParseException) { return false; }
            catch (ArgumentException) { return false; }
        }
    }
}
