using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Common
{
    public static class Money
    {
        // كل مبلغ بيتقرّب مرة واحدة هنا قبل ما يتخزن، فمفيش فروق قروش بين الأعمدة
        public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
        public static decimal RoundPercent(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
    }

    // حدود تقنية (مش قواعد بيزنس): الهدف إن decimal(18,2) عمره ما يعمل Overflow.
    // غيّر الأرقام دي براحتك لو محتاج سعر أكبر.
    public static class Limits
    {
        public const decimal MaxPrice = 10_000_000m;
        public const int MaxQuantity = 1_000_000;
        public const decimal MaxLineTotal = 1_000_000_000_000m;
    }
}
