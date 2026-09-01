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

        builder.HasData(
            SeedFrame("11111111-1111-1111-1111-111111111111", "ARM-001", "Linha Visão", "Clássica", "Preto", FrameShape.Square, TargetAudience.Adult),
            SeedFrame("22222222-2222-2222-2222-222222222222", "ARM-002", "Linha Visão", "Leve", "Tartaruga", FrameShape.Round, TargetAudience.Adult),
            SeedFrame("33333333-3333-3333-3333-333333333333", "ARM-003", "Linha Visão", "Solar", "Preto", FrameShape.Aviator, TargetAudience.Adult, FrameType.Sunglasses),
            SeedFrame("44444444-4444-4444-4444-444444444444", "ARM-004", "Linha Visão", "Colorida", "Azul", FrameShape.Round, TargetAudience.Child),
            SeedFrame("55555555-5555-5555-5555-555555555555", "ARM-005", "Linha Visão", "Minimal", "Dourado", FrameShape.Other, TargetAudience.Adult));
    }

    private static object SeedFrame(
        string id,
        string code,
        string brand,
        string model,
        string color,
        FrameShape shape,
        TargetAudience targetAudience,
        FrameType type = FrameType.Prescription) => new
        {
            Id = Guid.Parse(id),
            Code = code,
            Brand = brand,
            Model = model,
            Color = color,
            Price = 219m,
            StockQuantity = 1,
            Type = type,
            Shape = shape,
            TargetAudience = targetAudience,
            IsActive = true,
            IsPublished = true
        };
}
