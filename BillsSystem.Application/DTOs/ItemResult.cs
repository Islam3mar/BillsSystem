using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.DTOs
{
    public class ItemResult
    {
        public bool Success { get; set; }
        public Item? Item { get; set; }

        public string? TypeError { get; set; }
        public string? UnitError { get; set; }
        public string? NameError { get; set; }
        public string? SellingPriceError { get; set; }
        public string? BuyingPriceError { get; set; }

        public bool HasErrors =>
            TypeError != null || UnitError != null || NameError != null ||
            SellingPriceError != null || BuyingPriceError != null;
    }
}
