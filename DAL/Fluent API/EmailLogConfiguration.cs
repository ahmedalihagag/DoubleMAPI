using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Fluent_API
{
    public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
    {
        public void Configure(EntityTypeBuilder<EmailLog> builder)
        {
            builder.HasKey(el => el.Id);

            builder.Property(el => el.To).IsRequired().HasMaxLength(200);
            builder.Property(el => el.Subject).IsRequired().HasMaxLength(300);
            builder.Property(el => el.Status).HasMaxLength(50).HasDefaultValue("Pending");
            builder.Property(el => el.Error).HasMaxLength(2000);
            builder.Property(el => el.RetryCount).HasDefaultValue(0);
            builder.Property(el => el.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(el => el.Status);
            builder.HasIndex(el => el.CreatedAt);
        }
    }
}
