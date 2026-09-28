using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;

namespace BillsSystem.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetAsync();
    }
}
