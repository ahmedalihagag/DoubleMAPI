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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(500);
            builder.Property(rt => rt.DeviceInfo).HasMaxLength(200);
            builder.Property(rt => rt.IpAddress).HasMaxLength(50);
            builder.Property(rt => rt.IsRevoked).HasDefaultValue(false);
            builder.Property(rt => rt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasIndex(rt => rt.UserId);
            builder.HasIndex(rt => rt.ExpiresAt);
            builder.HasIndex(rt => rt.IsRevoked);
        }
    }
}
