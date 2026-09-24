using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ValidateCredentialsAsync(string username, string password);
    }
}
