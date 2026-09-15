using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzCatalog.Infrastructure.EntityFramework.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductDbModel>
{
    private const int CurrencyCodeLength = 3;

    private const int PriceAmountPrecision = 18;

    public void Configure(EntityTypeBuilder<ProductDbModel> builder)
    {
        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Sku)
            .HasMaxLength(Sku.MaxLength)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.Property(product => product.Name)
            .HasMaxLength(ProductName.MaxLength)
            .IsRequired();

        builder.Property(product => product.PriceAmount)
            .HasPrecision(PriceAmountPrecision, Product.PriceMaxDecimalPlaces)
            .IsRequired();

        builder.Property(product => product.PriceCurrency)
            .HasMaxLength(CurrencyCodeLength)
            .IsRequired();
    }
}
