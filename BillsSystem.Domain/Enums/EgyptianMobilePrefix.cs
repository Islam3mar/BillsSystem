using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Enums
{
    // بادئات شبكات المحمول في مصر - القيمة هي آخر رقمين بعد الصفر (مثلاً Vodafone = 10 يعني "010")
    public enum EgyptianMobilePrefix
    {
        Vodafone = 10,
        Etisalat = 11,
        Orange = 12,
        WE = 15
    }
}
