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

    public class ParentStudentConfiguration : IEntityTypeConfiguration<ParentStudent>
    {
        public void Configure(EntityTypeBuilder<ParentStudent> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.HasOne(ps => ps.Parent)
                .WithMany(u => u.ParentsLinked)
                .HasForeignKey(ps => ps.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ps => ps.Student)
                .WithMany(u => u.StudentsLinked)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ps => new { ps.ParentId, ps.StudentId }).IsUnique();
            builder.HasIndex(ps => ps.StudentId);
            builder.HasIndex(ps => ps.IsActive);

            builder.Property(ps => ps.LinkedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(ps => ps.IsActive).HasDefaultValue(true);
        }
    }
}
