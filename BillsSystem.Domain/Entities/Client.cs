using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Helpers;

namespace BillsSystem.Domain.Entities
{
    public class Client : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        // خاصية محسوبة مش متخزنة في الداتابيز (Ignored في الـ Configuration)
        public string Network => EgyptianMobilePrefixHelper.GetNetworkName(Phone) ?? "-";
    }
}
