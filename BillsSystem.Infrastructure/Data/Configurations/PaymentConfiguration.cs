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

            builder.HasQueryFilter(p => !p.Bill.IsDeleted);
        }
    }
}
