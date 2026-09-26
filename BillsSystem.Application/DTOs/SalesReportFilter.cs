using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Application.DTOs
{
    public class SalesReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
