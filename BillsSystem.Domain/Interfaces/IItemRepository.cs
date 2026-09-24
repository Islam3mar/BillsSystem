using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IItemRepository : IGenericRepository<Item>
    {
        Task<bool> NameExistsInTypeAsync(int itemTypeId, string name, int? excludeId = null);
    }
}
