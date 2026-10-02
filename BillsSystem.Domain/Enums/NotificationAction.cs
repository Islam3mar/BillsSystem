using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Enums
{
    // اللي حصل (بيحدد اللون). Alert محجوزة لتنبيهات النظام زي تجاوز سقف الدين والفواتير المتأخرة
    public enum NotificationAction
    {
        Created = 1,
        Updated = 2,
        Deleted = 3,
        Alert = 4
    }
}
