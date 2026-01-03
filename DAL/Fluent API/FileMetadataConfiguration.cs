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
    public class FileMetadataConfiguration : IEntityTypeConfiguration<FileMetadata>
    {
        public void Configure(EntityTypeBuilder<FileMetadata> builder)
        {
            builder.HasKey(fm => fm.Id);

            builder.Property(fm => fm.FileName).IsRequired().HasMaxLength(300);
            builder.Property(fm => fm.FileType).IsRequired().HasMaxLength(50);
            builder.Property(fm => fm.BunnyCdnUrl).IsRequired().HasMaxLength(1000);
            builder.Property(fm => fm.LocalPath).HasMaxLength(500);
            builder.Property(fm => fm.IsDeleted).HasDefaultValue(false);
            builder.Property(fm => fm.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(fm => fm.Uploader)
                .WithMany()
                .HasForeignKey(fm => fm.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(fm => fm.FileType);
            builder.HasIndex(fm => fm.UploadedBy);
            builder.HasIndex(fm => fm.IsDeleted);
            builder.HasIndex(fm => fm.UploadedAt);
        }
    }
}
