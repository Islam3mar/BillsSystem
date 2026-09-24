using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class CategoryResult
    {
        public bool Success { get; set; }
        public Category? Category { get; set; }
        public string? TypeError { get; set; }
        public string? NameError { get; set; }

        public bool HasErrors => TypeError != null || NameError != null;
    }
}
