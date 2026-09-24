using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
            builder.Property(c => c.Notes).HasMaxLength(500);

            // اسم الـ Category يبقى Unique بس جوا نفس الـ Type
            builder.HasIndex(c => new { c.ItemTypeId, c.Name }).IsUnique();

            builder.HasOne(c => c.ItemType)
                   .WithMany(t => t.Categories)
                   .HasForeignKey(c => c.ItemTypeId)
                   .OnDelete(DeleteBehavior.Cascade);   // مسح الـ Type يمسح الـ Categories التابعة له
        }
    }
}
