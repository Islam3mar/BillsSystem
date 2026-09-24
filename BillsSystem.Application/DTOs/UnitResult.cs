using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class UnitResult
    {
        public bool Success { get; set; }
        public Unit? Unit { get; set; }
        public string? NameError { get; set; }

        public bool HasErrors => NameError != null;
    }
}
