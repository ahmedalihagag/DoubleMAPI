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
    public class StudentAnswerConfiguration : IEntityTypeConfiguration<StudentAnswer>
    {
        public void Configure(EntityTypeBuilder<StudentAnswer> builder)
        {
            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.PointsEarned).HasPrecision(5, 2);

            builder.HasOne(sa => sa.QuizAttempt)
                .WithMany(qa => qa.StudentAnswers)
                .HasForeignKey(sa => sa.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sa => sa.Question)
                .WithMany(q => q.StudentAnswers)
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(sa => new { sa.QuizAttemptId, sa.QuestionId });
            builder.HasIndex(sa => sa.QuestionId);
        }
    }
}
