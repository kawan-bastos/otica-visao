using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Engagement;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class FrameFavoriteConfiguration : IEntityTypeConfiguration<FrameFavorite>
{
    public void Configure(EntityTypeBuilder<FrameFavorite> builder)
    {
        builder.ToTable("frame_favorites");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.CustomerId).HasColumnName("customer_id");
        builder.Property(x => x.FrameId).HasColumnName("frame_id");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.HasIndex(x => new { x.CustomerId, x.FrameId }).IsUnique();
        builder.HasOne<Customer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Frame>().WithMany().HasForeignKey(x => x.FrameId).OnDelete(DeleteBehavior.Cascade);
    }
}
