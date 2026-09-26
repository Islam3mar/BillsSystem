using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;

namespace BillsSystem.Application.Interfaces
{
    public interface ISalesReportService
    {
        Task<SalesReportRequestResult> GenerateAsync(SalesReportFilter filter);
    }
}
