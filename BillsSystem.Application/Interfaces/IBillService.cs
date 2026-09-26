using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IBillService
    {
        Task<IEnumerable<Bill>> GetAllAsync();
        Task<Bill?> GetByIdAsync(int id);
        Task<BillResult> CreateAsync(BillInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
