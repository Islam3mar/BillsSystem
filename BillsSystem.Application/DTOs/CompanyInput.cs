using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class CompanyInput
    {
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
