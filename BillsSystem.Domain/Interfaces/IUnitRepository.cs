using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IUnitRepository : IGenericRepository<Unit>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
