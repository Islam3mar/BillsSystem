using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public int BillId { get; set; }
        public Bill Bill { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }
    }
}
