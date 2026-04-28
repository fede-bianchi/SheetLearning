using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LessonSlotConfiguration : IEntityTypeConfiguration<LessonSlot>
{
    public void Configure(EntityTypeBuilder<LessonSlot> builder)
    {
        builder.ToTable("lesson_slots");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(e => e.DataOraInizio)
            .HasColumnName("data_ora_inizio")
            .IsRequired();

        builder.Property(e => e.DataOraFine)
            .HasColumnName("data_ora_fine")
            .IsRequired();

        builder.Property(e => e.IsAvailable)
            .HasColumnName("is_available")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(e => e.Teacher)
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
