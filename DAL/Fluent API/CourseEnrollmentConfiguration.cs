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

    //Enrollment
    public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
    {
        public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
        {
            builder.HasKey(ce => ce.Id);

            builder.HasOne(ce => ce.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(ce => ce.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ce => ce.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(ce => ce.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ce => new { ce.StudentId, ce.CourseId }).IsUnique();
            builder.HasIndex(ce => ce.CourseId);
            builder.HasIndex(ce => ce.IsActive);

            builder.Property(ce => ce.EnrolledAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(ce => ce.IsActive).HasDefaultValue(true);
        }
    }
}
