using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        builder.HasKey(sale => sale.Id);
        builder.Property(sale => sale.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(sale => sale.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(sale => sale.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(sale => sale.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(sale => sale.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(sale => sale.CustomerId).HasDatabaseName("ix_sales_customer_id");
        builder.HasIndex(sale => new { sale.Status, sale.CreatedAtUtc }).HasDatabaseName("ix_sales_status_created_at");
        builder.HasOne(sale => sale.Customer).WithMany().HasForeignKey(sale => sale.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
