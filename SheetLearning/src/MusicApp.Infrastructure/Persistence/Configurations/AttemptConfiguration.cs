using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("attempts", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_attempts_punteggio", "punteggio >= 0 AND punteggio <= 100");
            tableBuilder.HasCheckConstraint("CK_attempts_difficolta", "difficolta IS NULL OR (difficolta >= 0 AND difficolta <= 30)");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.ExerciseTypeId)
            .HasColumnName("exercise_type_id")
            .IsRequired();

        builder.Property(e => e.LevelId)
            .HasColumnName("level_id");

        builder.Property(e => e.Difficolta)
            .HasColumnName("difficolta")
            .HasColumnType("TINYINT UNSIGNED");

        builder.Property(e => e.Punteggio)
            .HasColumnName("punteggio")
            .IsRequired();

        builder.Property(e => e.TempoRispostaMs)
            .HasColumnName("tempo_risposta_ms");

        builder.Property(e => e.ExerciseSubtype)
            .HasColumnName("exercise_subtype")
            .HasMaxLength(30);

        builder.Property(e => e.ToleranceWindowMs)
            .HasColumnName("tolerance_window_ms");

        builder.Property(e => e.InputSource)
            .HasColumnName("input_source")
            .HasColumnType("ENUM('mouse','keyboard','midi')")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExerciseType)
            .WithMany()
            .HasForeignKey(e => e.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Level)
            .WithMany()
            .HasForeignKey(e => e.LevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
