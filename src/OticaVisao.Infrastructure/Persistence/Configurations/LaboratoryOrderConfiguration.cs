using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.LaboratoryOrders;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class LaboratoryOrderConfiguration : IEntityTypeConfiguration<LaboratoryOrder>
{
    public void Configure(EntityTypeBuilder<LaboratoryOrder> builder)
    {
        builder.ToTable("laboratory_orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(order => order.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(order => order.SaleItemId).HasColumnName("sale_item_id").IsRequired();
        builder.Property(order => order.Laboratory).HasColumnName("laboratory").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(order => order.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(order => order.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
        builder.Property(order => order.Notes).HasColumnName("notes").HasMaxLength(1000);
        builder.Property(order => order.DocumentFileName).HasColumnName("document_file_name").HasMaxLength(80);
        builder.Property(order => order.SentAtUtc).HasColumnName("sent_at_utc");
        builder.Property(order => order.ReadyAtUtc).HasColumnName("ready_at_utc");
        builder.Property(order => order.DeliveredAtUtc).HasColumnName("delivered_at_utc");
        builder.Property(order => order.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(order => order.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(order => order.SaleItemId).IsUnique().HasDatabaseName("ux_laboratory_orders_sale_item_id");
        builder.HasIndex(order => new { order.Status, order.ExpectedDeliveryDate }).HasDatabaseName("ix_laboratory_orders_status_expected_date");
        builder.HasOne(order => order.Sale).WithMany().HasForeignKey(order => order.SaleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(order => order.SaleItem).WithMany().HasForeignKey(order => order.SaleItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(order => order.History).WithOne().HasForeignKey(history => history.LaboratoryOrderId).OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.History).HasField("history").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
