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
    public class BlacklistConfiguration : IEntityTypeConfiguration<Blacklist>
    {
        public void Configure(EntityTypeBuilder<Blacklist> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Reason).IsRequired().HasMaxLength(500);
            builder.Property(b => b.BlockType).HasMaxLength(50).HasDefaultValue("Permanent");
            builder.Property(b => b.BlockedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(b => b.User)
                .WithMany(u => u.BlacklistEntries)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.BlockedByUser)
                .WithMany()
                .HasForeignKey(b => b.BlockedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.BlockedAt);
            builder.HasIndex(b => b.ExpiresAt);
        }
    }
}
