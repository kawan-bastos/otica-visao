using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Authentication;

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
        builder.Property(customer => customer.Cpf).HasColumnName("cpf").HasMaxLength(11);
        builder.Property(customer => customer.BirthDate).HasColumnName("birth_date").HasColumnType("date");
        builder.Property(customer => customer.AccountUserId).HasColumnName("account_user_id");
        builder.Property(customer => customer.Email).HasColumnName("email").HasMaxLength(160);
        builder.Property(customer => customer.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(customer => customer.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(customer => customer.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(customer => customer.Name).HasDatabaseName("ix_customers_name");
        builder.HasIndex(customer => customer.Phone).HasDatabaseName("ix_customers_phone");
        builder.HasIndex(customer => customer.Cpf).IsUnique().HasDatabaseName("ix_customers_cpf");
        builder.HasIndex(customer => customer.AccountUserId).IsUnique().HasDatabaseName("ix_customers_account_user_id");
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Customer>(customer => customer.AccountUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(customer => customer.Address, address =>
        {
            address.Property(value => value.PostalCode).HasColumnName("postal_code").HasMaxLength(9).IsRequired(false);
            address.Property(value => value.Street).HasColumnName("street").HasMaxLength(160).IsRequired(false);
            address.Property(value => value.Number).HasColumnName("address_number").HasMaxLength(20).IsRequired(false);
            address.Property(value => value.Complement).HasColumnName("address_complement").HasMaxLength(80);
            address.Property(value => value.Neighborhood).HasColumnName("neighborhood").HasMaxLength(100).IsRequired(false);
            address.Property(value => value.City).HasColumnName("city").HasMaxLength(100).IsRequired(false);
            address.Property(value => value.State).HasColumnName("state").HasMaxLength(2).IsRequired(false);
        });
        builder.Navigation(customer => customer.Address).IsRequired(false);
    }
}
