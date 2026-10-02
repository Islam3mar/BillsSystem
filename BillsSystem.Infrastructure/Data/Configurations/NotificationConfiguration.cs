using System;
using System.Collections.Generic;
using System.Text;
using BillsSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillsSystem.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);

            // عداد غير المقروء + ترتيب الأحدث أول، وهم أكتر استعلامين بيتنفذوا (كل 30 ثانية)
            builder.HasIndex(n => new { n.IsRead, n.CreatedAt });
            builder.HasIndex(n => n.CreatedAt);
        }
    }
}
