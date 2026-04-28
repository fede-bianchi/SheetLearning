using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class LessonBundlePurchaseConfiguration : IEntityTypeConfiguration<LessonBundlePurchase>
{
    public void Configure(EntityTypeBuilder<LessonBundlePurchase> builder)
    {
        builder.ToTable("lesson_bundle_purchases");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.BundleId)
            .HasColumnName("bundle_id")
            .IsRequired();

        builder.Property(e => e.LezioniTotali)
            .HasColumnName("lezioni_totali")
            .IsRequired();

        builder.Property(e => e.LezioniUsate)
            .HasColumnName("lezioni_usate")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Bundle)
            .WithMany()
            .HasForeignKey(e => e.BundleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Payment)
            .WithMany()
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
