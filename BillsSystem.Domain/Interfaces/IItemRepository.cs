using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IItemRepository : IGenericRepository<Item>
    {
        Task<bool> NameExistsInTypeAsync(int itemTypeId, string name, int? excludeId = null);

        // Query واحد لكل الأصناف (Tracked) بدل GetByIdAsync جوه Loop
        Task<List<Item>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
