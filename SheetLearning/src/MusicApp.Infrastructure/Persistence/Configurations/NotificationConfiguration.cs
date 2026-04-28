using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.Tipo)
            .HasColumnName("tipo")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Titolo)
            .HasColumnName("titolo")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Corpo)
            .HasColumnName("corpo")
            .HasColumnType("TEXT");

        builder.Property(e => e.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(50);

        builder.Property(e => e.TargetId)
            .HasColumnName("target_id");

        builder.Property(e => e.IsRead)
            .HasColumnName("is_read")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.IsArchived)
            .HasColumnName("is_archived")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.ReadAt)
            .HasColumnName("read_at");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
