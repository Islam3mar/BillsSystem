using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
