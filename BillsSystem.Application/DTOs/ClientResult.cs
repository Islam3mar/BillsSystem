using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class ClientResult
    {
        public bool Success { get; set; }
        public Client? Client { get; set; }

        public string? NameError { get; set; }
        public string? PhoneError { get; set; }
        public string? AddressError { get; set; }

        public bool HasErrors => NameError != null || PhoneError != null || AddressError != null;
    }
}
