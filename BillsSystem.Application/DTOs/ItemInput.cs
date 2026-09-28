using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class ItemInput
    {
        public int ItemTypeId { get; set; }
        public int UnitId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal BuyingPrice { get; set; }
        public int QuantityInStock { get; set; }
        public string? Notes { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
