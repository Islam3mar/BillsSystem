using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using BillsSystem.Domain.Interfaces;
using BillsSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BillsSystem.Infrastructure.Repositories
{
    public class ClientRepository : BaseRepository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await Query.AnyAsync(c =>
                c.Name.ToLower() == normalized && (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        // نفس الإيميل مينفعش يتكرر على عميلين (من غير حساسية لحالة الحروف)
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var normalized = email.Trim().ToLower();
            return await Query.AnyAsync(c =>
                c.Email != null && c.Email.ToLower() == normalized && (!excludeId.HasValue || c.Id != excludeId.Value));
        }
    }
}
