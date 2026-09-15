using MainServer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MainServer.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.HasIndex(p => p.CategoryId);
    }
}
