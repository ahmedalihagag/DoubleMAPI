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

    // PARENT-STUDENT RELATIONSHIP
    public class ParentLinkCodeConfiguration : IEntityTypeConfiguration<ParentLinkCode>
    {
        public void Configure(EntityTypeBuilder<ParentLinkCode> builder)
        {
            builder.HasKey(plc => plc.Code);

            builder.Property(plc => plc.Code).HasMaxLength(20);

            builder.HasOne(plc => plc.Student)
                .WithMany(u => u.LinkCodes)
                .HasForeignKey(plc => plc.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(plc => plc.StudentId);
            builder.HasIndex(plc => plc.ExpiresAt);
            builder.HasIndex(plc => plc.IsUsed);

            builder.Property(plc => plc.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(plc => plc.IsUsed).HasDefaultValue(false);
        }
    }
}
