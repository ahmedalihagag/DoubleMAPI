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
    public class CourseProgressConfiguration : IEntityTypeConfiguration<CourseProgress>
    {
        public void Configure(EntityTypeBuilder<CourseProgress> builder)
        {
            builder.HasKey(cp => cp.Id);

            builder.Property(cp => cp.CompletionPercentage)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.HasOne(cp => cp.Student)
                .WithMany(u => u.CourseProgresses)
                .HasForeignKey(cp => cp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cp => cp.Course)
                .WithMany(c => c.CourseProgresses)
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cp => new { cp.StudentId, cp.CourseId }).IsUnique();
            builder.HasIndex(cp => cp.CourseId);

            builder.Property(cp => cp.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
