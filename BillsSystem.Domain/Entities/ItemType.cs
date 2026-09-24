using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class ItemType : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Notes { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
