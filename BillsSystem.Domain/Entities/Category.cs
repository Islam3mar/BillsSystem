using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Notes { get; set; }

        public int ItemTypeId { get; set; }
        public ItemType ItemType { get; set; } = null!;
    }
}
