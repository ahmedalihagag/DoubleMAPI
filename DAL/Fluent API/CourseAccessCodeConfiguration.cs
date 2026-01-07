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

            // Required Properties
            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(c => c.CourseId)
                   .IsRequired();

            builder.Property(c => c.CreatedBy)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.Property(c => c.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.ExpiresAt)
                   .IsRequired();

            builder.Property(c => c.IsUsed)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(c => c.IsDisabled)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Optional Properties
            builder.Property(c => c.UsedAt)
                   .IsRequired(false);

            builder.Property(c => c.UsedBy)
                   .HasMaxLength(450)
                   .IsRequired(false);

            builder.Property(c => c.DisabledAt)
                   .IsRequired(false);

            // Relationships
            builder.HasOne(c => c.Course)
                   .WithMany(c => c.AccessCodes)
                   .HasForeignKey(c => c.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(c => c.Code)
                   .IsUnique();

            builder.HasIndex(c => new { c.CourseId, c.IsUsed, c.IsDisabled })
                   .HasDatabaseName("IX_CourseAccessCode_CourseId_IsUsed_IsDisabled");

            builder.HasIndex(c => c.ExpiresAt)
                   .HasDatabaseName("IX_CourseAccessCode_ExpiresAt");

            builder.HasIndex(c => c.CreatedAt)
                   .HasDatabaseName("IX_CourseAccessCode_CreatedAt");
        }
    }
}
