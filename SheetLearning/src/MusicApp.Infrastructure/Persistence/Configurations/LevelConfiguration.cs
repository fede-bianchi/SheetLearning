using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> builder)
    {
        builder.ToTable("levels");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ExerciseTypeId)
            .HasColumnName("exercise_type_id")
            .IsRequired();

        builder.Property(e => e.ClefId)
            .HasColumnName("clef_id");

        builder.Property(e => e.NumeroLivello)
            .HasColumnName("numero_livello")
            .HasColumnType("TINYINT UNSIGNED")
            .IsRequired();

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Descrizione)
            .HasColumnName("descrizione")
            .HasColumnType("TEXT");

        builder.Property(e => e.PunteggioMinimoSblocco)
            .HasColumnName("punteggio_minimo_sblocco")
            .IsRequired();

        builder.HasOne(e => e.ExerciseType)
            .WithMany()
            .HasForeignKey(e => e.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Clef)
            .WithMany()
            .HasForeignKey(e => e.ClefId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
