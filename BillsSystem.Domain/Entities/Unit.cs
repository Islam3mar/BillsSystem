using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class Unit : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
