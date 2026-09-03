using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Auditing;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Id).HasColumnName("id");
        builder.Property(log => log.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(log => log.EntityType).HasColumnName("entity_type").HasMaxLength(80).IsRequired();
        builder.Property(log => log.EntityId).HasColumnName("entity_id").HasMaxLength(80).IsRequired();
        builder.Property(log => log.Description).HasColumnName("description").HasMaxLength(300).IsRequired();
        builder.Property(log => log.PerformedBy).HasColumnName("performed_by").HasMaxLength(200).IsRequired();
        builder.Property(log => log.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.HasIndex(log => log.OccurredAtUtc);
    }
}
