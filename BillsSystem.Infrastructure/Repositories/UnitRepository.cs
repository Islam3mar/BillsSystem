using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace BillsSystem.Infrastructure.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        public UnitRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await Query.AnyAsync(u =>
                u.Name.ToLower() == normalized && (!excludeId.HasValue || u.Id != excludeId.Value));
        }
    }
}
