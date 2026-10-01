using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.PaymentDate).HasColumnType("date");
            builder.Property(p => p.Notes).HasMaxLength(200);
            builder.Property(p => p.VoidReason).HasMaxLength(200);

            builder.Property(p => p.Method).HasConversion<int>();
            builder.Property(p => p.StripeSessionId).HasMaxLength(100);
            builder.Property(p => p.StripePaymentIntentId).HasMaxLength(100);

            // مفيش فاتورة تتسجلها Session بتاعة Stripe مرتين (Webhook بيتكرر أحيانًا)
            builder.HasIndex(p => p.StripeSessionId)
                   .IsUnique()
                   .HasFilter("[StripeSessionId] IS NOT NULL");

            builder.HasQueryFilter(p => !p.Bill.IsDeleted);
        }
    }
}
