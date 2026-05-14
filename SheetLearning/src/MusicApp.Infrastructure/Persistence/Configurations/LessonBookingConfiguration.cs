using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LessonBookingConfiguration : IEntityTypeConfiguration<LessonBooking>
{
    public void Configure(EntityTypeBuilder<LessonBooking> builder)
    {
        builder.ToTable("lesson_bookings");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(e => e.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(e => e.SlotId)
            .HasColumnName("slot_id")
            .IsRequired();

        builder.Property(e => e.BundlePurchaseId)
            .HasColumnName("bundle_purchase_id");

        builder.Property(e => e.Stato)
            .HasColumnName("stato")
            .HasColumnType("ENUM('proposta','confermata','cancellata','completata')")
            .IsRequired();

        builder.Property(e => e.NoteStudente)
            .HasColumnName("note_studente")
            .HasColumnType("TEXT");

        builder.Property(e => e.NoteInsegnante)
            .HasColumnName("note_insegnante")
            .HasColumnType("TEXT");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Teacher)
            .WithMany()
            .HasForeignKey(e => e.TeacherId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Slot)
            .WithMany()
            .HasForeignKey(e => e.SlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.BundlePurchase)
            .WithMany()
            .HasForeignKey(e => e.BundlePurchaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.TeacherId, b.Stato })
              .HasDatabaseName("IX_LessonBookings_TeacherStato");

        builder.HasIndex(b => new { b.StudentId, b.Stato })
              .HasDatabaseName("IX_LessonBookings_StudentStato");
    }
}
