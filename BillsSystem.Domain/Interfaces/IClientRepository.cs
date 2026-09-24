using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;

namespace BillsSystem.Domain.Interfaces
{
    public interface IClientRepository : IGenericRepository<Client>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
