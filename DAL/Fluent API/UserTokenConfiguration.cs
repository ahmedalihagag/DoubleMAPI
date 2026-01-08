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
    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            // Table name
            builder.ToTable("UserTokens");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Token)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(x => x.TokenType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Expiration)
                   .IsRequired();

            builder.Property(x => x.IsUsed)
                   .IsRequired()
                   .HasDefaultValue(false);

            // Indexes (VERY IMPORTANT for performance & security)
            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.TokenType });

            builder.HasIndex(x => x.Expiration);
            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.TokenType });
            builder.HasIndex(x => x.Expiration);
            builder.Property(x => x.IsUsed)
                   .HasDefaultValue(false);
            builder.Property(x => x.TokenType)
                   .HasConversion<string>()
                   .HasMaxLength(50);


            // Relationship (ONLY if you have ApplicationUser navigation)
            // Uncomment if UserToken has navigation property
            /*
            builder.HasOne<ApplicationUser>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            */
        }
    }
}
