using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class ItemTypeResult
    {
        public bool Success { get; set; }
        public ItemType? ItemType { get; set; }
        public string? CompanyError { get; set; }
        public string? NameError { get; set; }

        public bool HasErrors => CompanyError != null || NameError != null;
    }
}
