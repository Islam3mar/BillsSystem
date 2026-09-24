using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IItemTypeService
    {
        Task<IEnumerable<ItemType>> GetAllAsync();
        Task<ItemType?> GetByIdAsync(int id);
        Task<IEnumerable<ItemType>> GetByCompanyAsync(int companyId);
        Task<ItemTypeResult> CreateAsync(ItemTypeInput input);
        Task<ItemTypeResult> UpdateAsync(int id, ItemTypeInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
