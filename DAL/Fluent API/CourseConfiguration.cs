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

    //Course
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            builder.Property(c => c.Description).HasMaxLength(2000);
            builder.Property(c => c.ImageUrl).HasMaxLength(500);
            builder.Property(c => c.Category).HasMaxLength(50);
            builder.Property(c => c.Level).HasMaxLength(50);
            builder.Property(c => c.IsPublished).HasDefaultValue(false);
            builder.Property(c => c.IsDeleted).HasDefaultValue(false);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(c => c.Teacher)
                .WithMany(u => u.CoursesAsTeacher)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => c.TeacherId);
            builder.HasIndex(c => c.IsPublished);
            builder.HasIndex(c => c.IsDeleted);
            builder.HasIndex(c => c.Category);
            builder.HasIndex(c => c.Level);
        }
    }
}
