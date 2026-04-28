using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class UserLevelProgressConfiguration : IEntityTypeConfiguration<UserLevelProgress>
{
    public void Configure(EntityTypeBuilder<UserLevelProgress> builder)
    {
        builder.ToTable("user_level_progress");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.LevelId)
            .HasColumnName("level_id")
            .IsRequired();

        builder.Property(e => e.Sbloccato)
            .HasColumnName("sbloccato")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.Completato)
            .HasColumnName("completato")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.DataSblocco)
            .HasColumnName("data_sblocco");

        builder.Property(e => e.DataCompletamento)
            .HasColumnName("data_completamento");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Level)
            .WithMany()
            .HasForeignKey(e => e.LevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
