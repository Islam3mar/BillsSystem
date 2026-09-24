using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class ItemTypeConfiguration : IEntityTypeConfiguration<ItemType>
    {
        public void Configure(EntityTypeBuilder<ItemType> builder)
        {
            builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
            builder.Property(t => t.Notes).HasMaxLength(500);

            // اسم الـ Type يبقى Unique بس جوا نفس الـ Company (مش عالمستوى العام)
            builder.HasIndex(t => new { t.CompanyId, t.Name }).IsUnique();

            builder.HasOne(t => t.Company)
                   .WithMany()
                   .HasForeignKey(t => t.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade);   // مسح الـ Company يمسح الـ Types التابعة له
        }
    }
}
