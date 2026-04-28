using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LessonRatingConfiguration : IEntityTypeConfiguration<LessonRating>
{
    public void Configure(EntityTypeBuilder<LessonRating> builder)
    {
        builder.ToTable("lesson_ratings");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.BookingId)
            .HasColumnName("booking_id")
            .IsRequired();

        builder.Property(e => e.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(e => e.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(e => e.Valutazione)
            .HasColumnName("valutazione")
            .HasColumnType("TINYINT UNSIGNED")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => e.BookingId)
            .IsUnique();

        builder.HasOne(e => e.Booking)
            .WithOne(e => e.Rating)
            .HasForeignKey<LessonRating>(e => e.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}
