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
    public class CourseCodeConfiguration : IEntityTypeConfiguration<CourseCode>
    {
        public void Configure(EntityTypeBuilder<CourseCode> builder)
        {
            builder.HasKey(cc => cc.Code);

            builder.Property(cc => cc.Code).HasMaxLength(20);

            builder.HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCodes)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cc => cc.Issuer)
                .WithMany(u => u.IssuedCourseCodes)
                .HasForeignKey(cc => cc.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(cc => cc.CourseId);
            builder.HasIndex(cc => cc.ExpiresAt);
            builder.HasIndex(cc => cc.IsUsed);

            builder.Property(cc => cc.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(cc => cc.IsUsed).HasDefaultValue(false);
        }
    }
}
