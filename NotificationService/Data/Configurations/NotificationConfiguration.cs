using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationService.Entities;

namespace NotificationService.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title).HasMaxLength(128).IsRequired();
        builder.Property(n => n.Message).HasMaxLength(512).IsRequired();
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(32);
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(n => n.OrderId);
        builder.HasIndex(n => new { n.PaymentId, n.Type }).IsUnique();
    }
}
