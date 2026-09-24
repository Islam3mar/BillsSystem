using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class ItemRepository : BaseRepository<Item>, IItemRepository
    {
        public ItemRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Item>> GetAllAsync()
            => await Query.Include(i => i.ItemType).ThenInclude(t => t.Company)
                          .Include(i => i.Unit)
                          .AsNoTracking().ToListAsync();

        public override async Task<Item?> GetByIdAsync(int id)
            => await Query.Include(i => i.ItemType).ThenInclude(t => t.Company)
                          .Include(i => i.Unit)
                          .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<bool> NameExistsInTypeAsync(int itemTypeId, string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await Query.AnyAsync(i =>
                i.ItemTypeId == itemTypeId &&
                i.Name.ToLower() == normalized &&
                (!excludeId.HasValue || i.Id != excludeId.Value));
        }
    }
}
