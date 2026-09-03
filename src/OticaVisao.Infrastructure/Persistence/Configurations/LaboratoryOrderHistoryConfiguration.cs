using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.LaboratoryOrders;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class LaboratoryOrderHistoryConfiguration : IEntityTypeConfiguration<LaboratoryOrderHistory>
{
    public void Configure(EntityTypeBuilder<LaboratoryOrderHistory> builder)
    {
        builder.ToTable("laboratory_order_history");
        builder.HasKey(history => history.Id);
        builder.Property(history => history.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(history => history.LaboratoryOrderId).HasColumnName("laboratory_order_id").IsRequired();
        builder.Property(history => history.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(history => history.ChangedAtUtc).HasColumnName("changed_at_utc").IsRequired();
        builder.HasIndex(history => new { history.LaboratoryOrderId, history.ChangedAtUtc }).HasDatabaseName("ix_laboratory_order_history_order_date");
    }
}
