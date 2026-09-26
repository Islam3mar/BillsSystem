using System;
using System.Collections.Generic;
using System.Text;

namespace BillsSystem.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICompanyRepository Companies { get; }
        IItemTypeRepository ItemTypes { get; }
        ICategoryRepository Categories { get; }
        IUnitRepository Units { get; }
        IItemRepository Items { get; }
        IClientRepository Clients { get; }
        IBillRepository Bills { get; }

        Task<int> SaveChangesAsync();
    }
}
