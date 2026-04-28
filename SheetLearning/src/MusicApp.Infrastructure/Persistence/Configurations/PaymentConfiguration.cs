using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(e => e.Importo)
            .HasColumnName("importo")
            .HasColumnType("DECIMAL(10,2)")
            .IsRequired();

        builder.Property(e => e.Valuta)
            .HasColumnName("valuta")
            .HasMaxLength(3)
            .HasDefaultValue("EUR")
            .IsRequired();

        builder.Property(e => e.Stato)
            .HasColumnName("stato")
            .HasColumnType("ENUM('pending','completed','failed','refunded')")
            .IsRequired();

        builder.Property(e => e.MetodoPagamento)
            .HasColumnName("metodo_pagamento")
            .HasMaxLength(50);

        builder.Property(e => e.RiferimentoEsterno)
            .HasColumnName("riferimento_esterno")
            .HasMaxLength(255);

        builder.Property(e => e.Tipo)
            .HasColumnName("tipo")
            .HasColumnType("ENUM('abbonamento_pro','bundle_lezioni','lezione_singola')")
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

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
