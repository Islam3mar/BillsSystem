using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class BillItemConfiguration : IEntityTypeConfiguration<BillItem>
    {
        public void Configure(EntityTypeBuilder<BillItem> builder)
        {
            builder.Property(i => i.SellingPrice).HasColumnType("decimal(18,2)");
            builder.Property(i => i.Discount).HasColumnType("decimal(18,2)");
            builder.Property(i => i.Total).HasColumnType("decimal(18,2)");
            builder.Property(i => i.DiscountAmount).HasColumnType("decimal(18,2)");
            builder.Property(i => i.Balance).HasColumnType("decimal(18,2)");
            builder.Property(i => i.BuyingPrice).HasColumnType("decimal(18,2)");
            builder.Property(i => i.DiscountType).HasConversion<int>();

            builder.Property(i => i.ItemName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.TypeName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.CompanyName).IsRequired().HasMaxLength(150);
            builder.Property(i => i.UnitName).IsRequired().HasMaxLength(150);

            builder.HasQueryFilter(i => !i.Bill.IsDeleted);

            builder.HasOne(i => i.Item)
                   .WithMany()
                   .HasForeignKey(i => i.ItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
