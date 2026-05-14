using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.Token)
            .HasColumnName("token")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(255);

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasColumnType("TEXT");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => e.Token)
            .IsUnique();

        builder.HasIndex(s => new { s.UserId, s.ExpiresAt })
              .HasDatabaseName("IX_Sessions_UserExpiry");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
