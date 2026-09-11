using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Engagement;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class FrameReservationConfiguration : IEntityTypeConfiguration<FrameReservation>
{
    public void Configure(EntityTypeBuilder<FrameReservation> builder)
    {
        builder.ToTable("frame_reservations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.FrameId).HasColumnName("frame_id");
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.VisitPeriod).HasColumnName("visit_period").HasMaxLength(40);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.ExpiresAtUtc).HasColumnName("expires_at_utc");
        builder.Property(x => x.EndedAtUtc).HasColumnName("ended_at_utc");
        builder.Property(x => x.SaleId).HasColumnName("sale_id");
        builder.HasIndex(x => new { x.CustomerId, x.Status });
        builder.HasIndex(x => new { x.FrameId, x.Status });
        builder.HasIndex(x => new { x.CustomerId, x.FrameId })
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ux_frame_reservations_active_customer_frame");
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Frame>().WithMany().HasForeignKey(x => x.FrameId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Sale>().WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.SetNull);
    }
}
