using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.Property(i => i.Name).IsRequired().HasMaxLength(150);
            builder.Property(i => i.Notes).HasMaxLength(500);

            builder.Property(i => i.SellingPrice).HasColumnType("decimal(18,2)");
            builder.Property(i => i.BuyingPrice).HasColumnType("decimal(18,2)");

            // اسم الـ Item يبقى Unique بس جوا نفس الـ Type
            builder.HasIndex(i => new { i.ItemTypeId, i.Name }).IsUnique();

            builder.HasOne(i => i.ItemType)
                   .WithMany()
                   .HasForeignKey(i => i.ItemTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Unit)
                   .WithMany()
                   .HasForeignKey(i => i.UnitId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
