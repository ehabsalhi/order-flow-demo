using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentService.Entities;

namespace PaymentService.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.TransactionId).HasMaxLength(64);

        builder.Property(p => p.Status).HasConversion<string>();

        builder.Property(p => p.Provider).HasConversion<string>();

        builder.HasIndex(p => p.OrderId);
    }
}
