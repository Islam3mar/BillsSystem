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
            builder.Property(b => b.BillDate).HasColumnType("date");   // تاريخ بس، من غير وقت
            builder.HasIndex(b => b.BillDate);                         // للتقارير والبحث بالتاريخ

            builder.Property(b => b.BillsTotal).HasColumnType("decimal(18,2)");
            builder.Property(b => b.PercentageDiscount).HasColumnType("decimal(7,4)");   // كانت (18,2) وبتقطع الخانات
            builder.Property(b => b.ValueDiscount).HasColumnType("decimal(18,2)");
            builder.Property(b => b.TheNet).HasColumnType("decimal(18,2)");
            builder.Property(b => b.PaidUp).HasColumnType("decimal(18,2)");
            builder.Property(b => b.TheRest).HasColumnType("decimal(18,2)");
            builder.Property(b => b.DiscountType).HasConversion<int>();

            // التذكيرات
            builder.Property(b => b.DueDate).HasColumnType("date");
            builder.Property(b => b.LastReminderType).HasConversion<int>();
            builder.HasIndex(b => b.DueDate);

            builder.Property(b => b.RowVersion).IsRowVersion();

            builder.HasIndex(b => b.SubmissionId)
                   .IsUnique()
                   .HasFilter("[SubmissionId] IS NOT NULL");

            // الفواتير المحذوفة (Soft Delete) بتختفي من أي Query أوتوماتيك
            builder.HasQueryFilter(b => !b.IsDeleted);

            builder.HasOne(b => b.Client)
                   .WithMany()
                   .HasForeignKey(b => b.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.Items)
                   .WithOne(i => i.Bill)
                   .HasForeignKey(i => i.BillId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Payments)
                   .WithOne(p => p.Bill)
                   .HasForeignKey(p => p.BillId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
