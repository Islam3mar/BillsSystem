using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Notes { get; set; }

        public ICollection<ItemType> ItemTypes { get; set; } = new List<ItemType>();
    }
}
