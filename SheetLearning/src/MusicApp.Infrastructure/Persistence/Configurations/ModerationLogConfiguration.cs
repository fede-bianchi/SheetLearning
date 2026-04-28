using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class ModerationLogConfiguration : IEntityTypeConfiguration<ModerationLog>
{
    public void Configure(EntityTypeBuilder<ModerationLog> builder)
    {
        builder.ToTable("moderation_logs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AdminId)
            .HasColumnName("admin_id")
            .IsRequired();

        builder.Property(e => e.TargetUserId)
            .HasColumnName("target_user_id");

        builder.Property(e => e.Azione)
            .HasColumnName("azione")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.TargetType)
            .HasColumnName("target_type")
            .HasMaxLength(50);

        builder.Property(e => e.TargetId)
            .HasColumnName("target_id");

        builder.Property(e => e.Motivazione)
            .HasColumnName("motivazione")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(e => e.ContenutoRimosso)
            .HasColumnName("contenuto_rimosso")
            .HasColumnType("TEXT");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(e => e.Admin)
            .WithMany()
            .HasForeignKey(e => e.AdminId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetUser)
            .WithMany()
            .HasForeignKey(e => e.TargetUserId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
