using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.ChatId)
            .HasColumnName("chat_id")
            .IsRequired();

        builder.Property(e => e.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(e => e.Contenuto)
            .HasColumnName("contenuto")
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(e => e.Letto)
            .HasColumnName("letto")
            .HasColumnType("TINYINT(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Messages)
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ChatId, m.CreatedAt })
              .HasDatabaseName("IX_ChatMessages_ChatCreated");
    }
}
