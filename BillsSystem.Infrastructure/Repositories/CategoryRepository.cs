using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Category>> GetAllAsync()
     => await Query.Include(c => c.ItemType).ThenInclude(t => t.Company).AsNoTracking().ToListAsync();

        public async Task<bool> NameExistsInTypeAsync(int itemTypeId, string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await Query.AnyAsync(c =>
                c.ItemTypeId == itemTypeId &&
                c.Name.ToLower() == normalized &&
                (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        public override async Task<Category?> GetByIdAsync(int id)
    => await Query.Include(c => c.ItemType).FirstOrDefaultAsync(c => c.Id == id);
    }
}
