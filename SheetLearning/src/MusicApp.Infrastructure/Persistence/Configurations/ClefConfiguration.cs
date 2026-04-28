using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class ClefConfiguration : IEntityTypeConfiguration<Clef>
{
    public void Configure(EntityTypeBuilder<Clef> builder)
    {
        builder.ToTable("clefs");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.Nome)
            .IsUnique();
    }
}
