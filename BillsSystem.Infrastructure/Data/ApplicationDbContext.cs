using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Common;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BillsSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public DbSet<Company> Companies => Set<Company>();
        public DbSet<ItemType> ItemTypes => Set<ItemType>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Bill> Bills => Set<Bill>();
        public DbSet<BillItem> BillItems => Set<BillItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = AppClock.Now;   // توقيت مصر مش توقيت السيرفر

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
                else if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
