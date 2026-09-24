using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class Item : BaseEntity
    {
        public string Name { get; set; } = null!;
        public decimal SellingPrice { get; set; }
        public decimal BuyingPrice { get; set; }
        public string? Notes { get; set; }

        public int ItemTypeId { get; set; }
        public ItemType ItemType { get; set; } = null!;

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
    }
}
