using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Nome)
            .HasColumnName("nome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Cognome)
            .HasColumnName("cognome")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Nickname)
            .HasColumnName("nickname")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.DataNascita)
            .HasColumnName("data_nascita")
            .IsRequired();

        builder.Property(e => e.Descrizione)
            .HasColumnName("descrizione")
            .HasColumnType("TEXT");

        builder.Property(e => e.Strumento)
            .HasColumnName("strumento")
            .HasMaxLength(255)
            .HasDefaultValue("Nessuno");

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(e => e.PlanId)
            .HasColumnName("plan_id")
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasIndex(e => e.Nickname)
            .IsUnique();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plan)
            .WithMany()
            .HasForeignKey(e => e.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
