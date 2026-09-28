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
        public int QuantityInStock { get; set; }
        public string? Notes { get; set; }

        public byte[] RowVersion { get; set; } = null!;

        public int ItemTypeId { get; set; }
        public ItemType ItemType { get; set; } = null!;

        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
    }
}
