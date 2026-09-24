using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class ItemTypeRepository : BaseRepository<ItemType>, IItemTypeRepository
    {
        public ItemTypeRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<ItemType>> GetAllAsync()
            => await Query.Include(t => t.Company).AsNoTracking().ToListAsync();

        public async Task<bool> NameExistsInCompanyAsync(int companyId, string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await Query.AnyAsync(t =>
                t.CompanyId == companyId &&
                t.Name.ToLower() == normalized &&
                (!excludeId.HasValue || t.Id != excludeId.Value));
        }

        public async Task<IEnumerable<ItemType>> GetByCompanyAsync(int companyId)
            => await Query.AsNoTracking().Where(t => t.CompanyId == companyId).OrderBy(t => t.Name).ToListAsync();
    }
}
