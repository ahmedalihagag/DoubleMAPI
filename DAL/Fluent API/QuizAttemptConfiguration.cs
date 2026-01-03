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
    public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.HasKey(qa => qa.Id);

            builder.Property(qa => qa.Score).HasPrecision(5, 2);
            builder.Property(qa => qa.MaxScore).HasPrecision(5, 2);
            builder.Property(qa => qa.Percentage).HasPrecision(5, 2);
            builder.Property(qa => qa.StartedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(qa => qa.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qa => qa.Student)
                .WithMany(u => u.QuizAttempts)
                .HasForeignKey(qa => qa.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(qa => qa.QuizId);
            builder.HasIndex(qa => qa.StudentId);
            builder.HasIndex(qa => qa.StartedAt);
        }
    }
}
