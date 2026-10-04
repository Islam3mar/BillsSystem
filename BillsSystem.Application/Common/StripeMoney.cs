using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.Common
{
    public static class StripeMoney
    {
        // العملات اللي Stripe بتتعامل معاها من غير كسور
        private static readonly HashSet<string> ZeroDecimal = new(StringComparer.OrdinalIgnoreCase)
        {
            "bif","clp","djf","gnf","jpy","kmf","krw","mga","pyg","rwf","ugx","vnd","vuv","xaf","xof","xpf"
        };

        public static long ToMinorUnits(decimal amount, string currency)
            => ZeroDecimal.Contains(currency)
                ? (long)Math.Round(amount, 0, MidpointRounding.AwayFromZero)
                : (long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);

        public static decimal FromMinorUnits(long minor, string currency)
            => ZeroDecimal.Contains(currency) ? minor : minor / 100m;
    }
}
