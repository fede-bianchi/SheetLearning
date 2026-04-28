using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LessonBundleConfiguration : IEntityTypeConfiguration<LessonBundle>
{
    public void Configure(EntityTypeBuilder<LessonBundle> builder)
    {
        builder.ToTable("lesson_bundles");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.NumeroLezioni)
            .HasColumnName("numero_lezioni")
            .IsRequired();

        builder.Property(e => e.Prezzo)
            .HasColumnName("prezzo")
            .HasColumnType("DECIMAL(10,2)")
            .IsRequired();

        builder.Property(e => e.ScontoPercentuale)
            .HasColumnName("sconto_percentuale")
            .HasColumnType("DECIMAL(5,2)")
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.ExpiresAfterDays)
            .HasColumnName("expires_after_days");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
