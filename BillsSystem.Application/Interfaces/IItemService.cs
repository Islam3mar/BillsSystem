using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface IItemService
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<ItemResult> CreateAsync(ItemInput input);
        Task<ItemResult> UpdateAsync(int id, ItemInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
