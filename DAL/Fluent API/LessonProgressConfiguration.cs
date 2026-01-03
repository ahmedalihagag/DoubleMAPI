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
    public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> builder)
        {
            builder.HasKey(lp => lp.Id);

            builder.HasOne(lp => lp.Student)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(lp => lp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(lp => new { lp.StudentId, lp.LessonId }).IsUnique();
            builder.HasIndex(lp => lp.LessonId);

            builder.Property(lp => lp.CompletedAt).HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
