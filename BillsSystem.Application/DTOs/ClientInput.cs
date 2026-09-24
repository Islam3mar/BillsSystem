using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class ClientInput
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
