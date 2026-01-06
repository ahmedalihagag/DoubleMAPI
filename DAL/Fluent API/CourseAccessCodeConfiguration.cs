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
    public class CourseAccessCodeConfiguration : IEntityTypeConfiguration<CourseAccessCode>
    {
        public void Configure(EntityTypeBuilder<CourseAccessCode> builder)
        {
            // Table name
            builder.ToTable("CourseAccessCodes");

            // Primary Key
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.IsUsed)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.CreatedBy)
                   .IsRequired()
                   .HasMaxLength(450);

            // Relationships
            builder.HasOne(c => c.Course)
                   .WithMany(c => c.AccessCodes) // Make sure Course has ICollection<CourseAccessCode> AccessCodes
                   .HasForeignKey(c => c.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Index for fast lookup by Code
            builder.HasIndex(c => c.Code)
                   .IsUnique();

            // Optional properties
            builder.Property(c => c.UsedAt)
                   .IsRequired(false);

            builder.Property(c => c.ExpiresAt)
                   .IsRequired(false);
        }
    }
}
