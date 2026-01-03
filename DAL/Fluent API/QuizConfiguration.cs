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
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Title).IsRequired().HasMaxLength(200);
            builder.Property(q => q.Description).HasMaxLength(1000);
            builder.Property(q => q.AllowReentry).HasDefaultValue(false);
            builder.Property(q => q.ShowCorrectAnswers).HasDefaultValue(true);
            builder.Property(q => q.ShuffleQuestions).HasDefaultValue(false);
            builder.Property(q => q.ShuffleOptions).HasDefaultValue(false);
            builder.Property(q => q.PassingScore).HasPrecision(5, 2);
            builder.Property(q => q.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(q => q.CourseId);
        }
    }
}
