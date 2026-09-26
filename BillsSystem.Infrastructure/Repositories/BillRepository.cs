using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class BillRepository : BaseRepository<Bill>, IBillRepository
    {
        public BillRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Bill>> GetAllAsync()
            => await Query.Include(b => b.Client)
                          .Include(b => b.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Unit)
                          .AsNoTracking()
                          .OrderByDescending(b => b.BillDate)
                          .ThenByDescending(b => b.Id)
                          .ToListAsync();

        public override async Task<Bill?> GetByIdAsync(int id)
            => await Query.Include(b => b.Client)
                          .Include(b => b.Items).ThenInclude(i => i.Item).ThenInclude(it => it.Unit)
                          .FirstOrDefaultAsync(b => b.Id == id);
    }
}
