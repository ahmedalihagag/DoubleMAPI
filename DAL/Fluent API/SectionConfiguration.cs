using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
namespace DAL.Fluent_API
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title).IsRequired().HasMaxLength(200);
            builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => new { s.CourseId, s.DisplayOrder });
        }
    }
}
