using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Enums
{
    // الكيان اللي حصل عليه الحدث (بيحدد الأيقونة واللينك)
    public enum NotificationType
    {
        Company = 1,
        Unit = 2,
        ItemType = 3,
        Category = 4,
        Item = 5,
        Client = 6,
        Bill = 7,
        Payment = 8
    }
}
