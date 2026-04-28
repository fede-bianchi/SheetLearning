using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class TeacherCategoryConfiguration : IEntityTypeConfiguration<TeacherCategory>
{
    public void Configure(EntityTypeBuilder<TeacherCategory> builder)
    {
        builder.ToTable("teacher_categories");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(e => e.Categoria)
            .HasColumnName("categoria")
            .HasColumnType("ENUM('principiante','intermedio','avanzato')")
            .IsRequired();

        builder.HasIndex(e => new { e.TeacherId, e.Categoria })
            .IsUnique();

        builder.HasOne(e => e.TeacherProfile)
            .WithMany(e => e.Categories)
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
