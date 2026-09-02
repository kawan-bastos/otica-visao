using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Customers;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(customer => customer.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(customer => customer.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Email).HasColumnName("email").HasMaxLength(160);
        builder.Property(customer => customer.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(customer => customer.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(customer => customer.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(customer => customer.Name).HasDatabaseName("ix_customers_name");
        builder.HasIndex(customer => customer.Phone).HasDatabaseName("ix_customers_phone");
    }
}
