using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Fluent_API
{
    public class DeviceSessionConfiguration : IEntityTypeConfiguration<DeviceSession>
    {
        public void Configure(EntityTypeBuilder<DeviceSession> builder)
        {
            builder.ToTable("DeviceSessions");
            builder.HasKey(d => d.Id);
            
            builder.Property(d => d.UserId)
                .IsRequired()
                .HasMaxLength(450);
            
            builder.Property(d => d.DeviceId)
                .IsRequired()
                .HasMaxLength(100);
            
            // ✅ ADD THIS LINE: Configure enum to string conversion
            builder.Property(d => d.ClientType)
                .IsRequired()
                .HasConversion<string>()  // ← Add this
                .HasMaxLength(20);         // ← Optional: add max length for the enum string
            
            builder.Property(d => d.DeviceInfo)
                .HasMaxLength(200);
            
            builder.Property(d => d.IpAddress)
                .HasMaxLength(50);
            
            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for performance
            builder.HasIndex(d => new { d.UserId, d.ClientType })
                .HasDatabaseName("IX_DeviceSession_UserId_ClientType");
            
            builder.HasIndex(d => new { d.UserId, d.IsActive })
                .HasDatabaseName("IX_DeviceSession_UserId_IsActive");
            
            builder.HasIndex(d => d.DeviceId);
        }
    }
}
