using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(e => e.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(e => e.DataInizio)
            .HasColumnName("data_inizio")
            .IsRequired();

        builder.Property(e => e.DataFine)
            .HasColumnName("data_fine")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.RinnovoAutomatico)
            .HasColumnName("rinnovo_automatico")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plan)
            .WithMany()
            .HasForeignKey(e => e.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Payment)
            .WithMany()
            .HasForeignKey(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
