using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class BillConfiguration : IEntityTypeConfiguration<Bill>
    {
        public void Configure(EntityTypeBuilder<Bill> builder)
        {
            builder.Property(b => b.BillsTotal).HasColumnType("decimal(18,2)");
            builder.Property(b => b.PercentageDiscount).HasColumnType("decimal(18,2)");
            builder.Property(b => b.ValueDiscount).HasColumnType("decimal(18,2)");
            builder.Property(b => b.TheNet).HasColumnType("decimal(18,2)");
            builder.Property(b => b.PaidUp).HasColumnType("decimal(18,2)");
            builder.Property(b => b.TheRest).HasColumnType("decimal(18,2)");
            builder.Property(b => b.DiscountType).HasConversion<int>();

            builder.HasOne(b => b.Client)
                   .WithMany()
                   .HasForeignKey(b => b.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);   // منمنعش مسح عميل ليه فواتير

            builder.HasMany(b => b.Items)
                   .WithOne(i => i.Bill)
                   .HasForeignKey(i => i.BillId)
                   .OnDelete(DeleteBehavior.Cascade);    // مسح الفاتورة يمسح بنودها
        }
    }
}
