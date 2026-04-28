using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class AttemptErrorConfiguration : IEntityTypeConfiguration<AttemptError>
{
    public void Configure(EntityTypeBuilder<AttemptError> builder)
    {
        builder.ToTable("attempt_errors");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        builder.Property(e => e.ElementType)
            .HasColumnName("element_type")
            .HasColumnType("ENUM('nota','accordo','ritmo')")
            .IsRequired();

        builder.Property(e => e.RispostaData)
            .HasColumnName("risposta_data")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.RispostaCorretta)
            .HasColumnName("risposta_corretta")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PosizioneNelPattern)
            .HasColumnName("posizione_nel_pattern")
            .HasColumnType("TINYINT UNSIGNED");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => e.AttemptId);

        builder.HasIndex(e => new { e.AttemptId, e.ElementType });

        builder.HasOne(e => e.Attempt)
            .WithMany(e => e.AttemptErrors)
            .HasForeignKey(e => e.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
