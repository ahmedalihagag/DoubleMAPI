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
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.Text).IsRequired().HasMaxLength(2000);
            builder.Property(q => q.QuestionType).HasMaxLength(50).HasDefaultValue("MultipleChoice");
            builder.Property(q => q.Points).HasPrecision(5, 2).HasDefaultValue(1);
            builder.Property(q => q.DifficultyLevel).HasMaxLength(50);
            builder.Property(q => q.Explanation).HasMaxLength(2000);

            builder.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(q => new { q.QuizId, q.DisplayOrder });
        }
    }
}
