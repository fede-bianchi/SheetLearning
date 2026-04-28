using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class BestScoreConfiguration : IEntityTypeConfiguration<BestScore>
{
    public void Configure(EntityTypeBuilder<BestScore> builder)
    {
        builder.ToTable("best_scores");

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

        builder.Property(e => e.PunteggioMigliore)
            .HasColumnName("punteggio_migliore")
            .IsRequired();

        builder.Property(e => e.AttemptId)
            .HasColumnName("attempt_id")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.ExerciseTypeId })
            .IsUnique();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ExerciseType)
            .WithMany()
            .HasForeignKey(e => e.ExerciseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Attempt)
            .WithMany()
            .HasForeignKey(e => e.AttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
