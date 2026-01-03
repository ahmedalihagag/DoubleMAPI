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
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title).IsRequired().HasMaxLength(200);
            builder.Property(l => l.VideoUrl).HasMaxLength(1000);
            builder.Property(l => l.MaterialUrl).HasMaxLength(1000);
            builder.Property(l => l.Description).HasMaxLength(5000);
            builder.Property(l => l.IsDeleted).HasDefaultValue(false);
            builder.Property(l => l.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => new { l.SectionId, l.DisplayOrder });
            builder.HasIndex(l => l.IsDeleted);
        }
    }
}
