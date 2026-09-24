using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Application.DTOs;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<CategoryResult> CreateAsync(CategoryInput input);
        Task<CategoryResult> UpdateAsync(int id, CategoryInput input);
        Task<(bool Success, string? Error)> DeleteAsync(int id);
    }
}
