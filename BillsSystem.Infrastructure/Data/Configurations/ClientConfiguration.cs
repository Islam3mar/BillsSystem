using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
            builder.HasIndex(c => c.Name).IsUnique();

            // رقم موبايل مصري: 11 رقم بالظبط
            builder.Property(c => c.Phone).IsRequired().HasMaxLength(11);
            builder.Property(c => c.Address).IsRequired().HasMaxLength(300);

            // خاصية محسوبة runtime بس - مش عمود في الجدول
            builder.Ignore(c => c.Network);
        }
    }
}
