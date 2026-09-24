using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IItemTypeRepository : IGenericRepository<ItemType>
    {
        Task<bool> NameExistsInCompanyAsync(int companyId, string name, int? excludeId = null);
        Task<IEnumerable<ItemType>> GetByCompanyAsync(int companyId);
    }
}
