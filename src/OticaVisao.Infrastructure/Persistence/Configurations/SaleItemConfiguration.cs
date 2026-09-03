using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(item => item.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(item => item.FrameId).HasColumnName("frame_id").IsRequired();
        builder.Property(item => item.FrameCode).HasColumnName("frame_code").HasMaxLength(40).IsRequired();
        builder.Property(item => item.FrameBrand).HasColumnName("frame_brand").HasMaxLength(100).IsRequired();
        builder.Property(item => item.FrameModel).HasColumnName("frame_model").HasMaxLength(100).IsRequired();
        builder.Property(item => item.FrameColor).HasColumnName("frame_color").HasMaxLength(80).IsRequired();
        builder.Property(item => item.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(item => item.IncludesLenses).HasColumnName("includes_lenses").IsRequired();
        builder.Property(item => item.FrameUnitPrice).HasColumnName("frame_unit_price").HasPrecision(10, 2).IsRequired();
        builder.Property(item => item.LensDescription).HasColumnName("lens_description").HasMaxLength(500);
        builder.Property(item => item.LensUnitPrice).HasColumnName("lens_unit_price").HasPrecision(10, 2).IsRequired();
        builder.Property(item => item.Laboratory).HasColumnName("laboratory").HasConversion<string>().HasMaxLength(30);
        builder.Ignore(item => item.UnitTotal);
        builder.Ignore(item => item.Total);
        builder.HasIndex(item => item.SaleId).HasDatabaseName("ix_sale_items_sale_id");
        builder.HasIndex(item => item.FrameId).HasDatabaseName("ix_sale_items_frame_id");
        builder.HasOne(item => item.Frame).WithMany().HasForeignKey(item => item.FrameId).OnDelete(DeleteBehavior.Restrict);
    }
}
