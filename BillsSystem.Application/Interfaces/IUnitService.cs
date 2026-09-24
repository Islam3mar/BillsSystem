using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IUnitService
    {
        Task<IEnumerable<Unit>> GetAllUnitsAsync();
        Task<Unit?> GetUnitByIdAsync(int id);
        Task<UnitResult> CreateUnitAsync(UnitInput input);
        Task<UnitResult> UpdateUnitAsync(int id, UnitInput input);
        Task<(bool Success, string? Error)> DeleteUnitAsync(int id);
    }
}
