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
            builder.Property(i => i.DiscountType).HasConversion<int>();

            // خصائص محسوبة runtime بس، مش أعمدة في الجدول
            builder.Ignore(i => i.Total);
            builder.Ignore(i => i.Balance);

            builder.HasOne(i => i.Item)
                   .WithMany()
                   .HasForeignKey(i => i.ItemId)
                   .OnDelete(DeleteBehavior.Restrict);   // منمنعش مسح صنف اتباع قبل كده
        }
    }
}
