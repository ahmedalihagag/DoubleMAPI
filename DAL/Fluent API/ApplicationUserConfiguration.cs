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

    // Configuration class for ApplicationUser entity
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FullName).HasMaxLength(100);
            builder.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            builder.Property(u => u.Language).HasMaxLength(5).HasDefaultValue("EN");
            builder.Property(u => u.DarkMode).HasDefaultValue(false);
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(u => u.Email);
            builder.HasIndex(u => u.IsActive);
        }
    }
}
