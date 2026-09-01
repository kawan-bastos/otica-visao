using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

internal sealed class FrameConfiguration : IEntityTypeConfiguration<Frame>
{
    public void Configure(EntityTypeBuilder<Frame> builder)
    {
        builder.ToTable("frames");

        builder.HasKey(frame => frame.Id);
        builder.Property(frame => frame.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(frame => frame.Code).HasColumnName("code").HasMaxLength(40).IsRequired();
        builder.HasIndex(frame => frame.Code).IsUnique().HasDatabaseName("ix_frames_code");

        builder.Property(frame => frame.Brand).HasColumnName("brand").HasMaxLength(100).IsRequired();
        builder.Property(frame => frame.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        builder.Property(frame => frame.Color).HasColumnName("color").HasMaxLength(80).IsRequired();
        builder.Property(frame => frame.Price).HasColumnName("price").HasPrecision(10, 2).IsRequired();
        builder.Property(frame => frame.StockQuantity).HasColumnName("stock_quantity").IsRequired();

        builder.Property(frame => frame.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(frame => frame.Shape)
            .HasColumnName("shape")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(frame => frame.TargetAudience)
            .HasColumnName("target_audience")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(frame => frame.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(frame => frame.IsPublished).HasColumnName("is_published").IsRequired();
        builder.Ignore(frame => frame.IsAvailable);

        builder.OwnsOne(frame => frame.Measurements, measurements =>
        {
            measurements.Property(value => value.LensWidthMillimeters).HasColumnName("lens_width_mm");
            measurements.Property(value => value.BridgeWidthMillimeters).HasColumnName("bridge_width_mm");
            measurements.Property(value => value.TempleLengthMillimeters).HasColumnName("temple_length_mm");
        });
    }
}
