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
        builder.Property(item => item.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
        builder.Property(item => item.FarRightSphere).HasColumnName("far_right_sphere").HasPrecision(5, 2);
        builder.Property(item => item.FarRightCylinder).HasColumnName("far_right_cylinder").HasPrecision(5, 2);
        builder.Property(item => item.FarRightAxis).HasColumnName("far_right_axis");
        builder.Property(item => item.FarRightDnp).HasColumnName("far_right_dnp").HasPrecision(5, 2);
        builder.Property(item => item.FarRightHeight).HasColumnName("far_right_height").HasPrecision(5, 2);
        builder.Property(item => item.FarRightAddition).HasColumnName("far_right_addition").HasPrecision(5, 2);
        builder.Property(item => item.FarLeftSphere).HasColumnName("far_left_sphere").HasPrecision(5, 2);
        builder.Property(item => item.FarLeftCylinder).HasColumnName("far_left_cylinder").HasPrecision(5, 2);
        builder.Property(item => item.FarLeftAxis).HasColumnName("far_left_axis");
        builder.Property(item => item.FarLeftDnp).HasColumnName("far_left_dnp").HasPrecision(5, 2);
        builder.Property(item => item.FarLeftHeight).HasColumnName("far_left_height").HasPrecision(5, 2);
        builder.Property(item => item.FarLeftAddition).HasColumnName("far_left_addition").HasPrecision(5, 2);
        builder.Property(item => item.NearRightSphere).HasColumnName("near_right_sphere").HasPrecision(5, 2);
        builder.Property(item => item.NearRightCylinder).HasColumnName("near_right_cylinder").HasPrecision(5, 2);
        builder.Property(item => item.NearRightAxis).HasColumnName("near_right_axis");
        builder.Property(item => item.NearRightDnp).HasColumnName("near_right_dnp").HasPrecision(5, 2);
        builder.Property(item => item.NearRightHeight).HasColumnName("near_right_height").HasPrecision(5, 2);
        builder.Property(item => item.NearRightAddition).HasColumnName("near_right_addition").HasPrecision(5, 2);
        builder.Property(item => item.NearLeftSphere).HasColumnName("near_left_sphere").HasPrecision(5, 2);
        builder.Property(item => item.NearLeftCylinder).HasColumnName("near_left_cylinder").HasPrecision(5, 2);
        builder.Property(item => item.NearLeftAxis).HasColumnName("near_left_axis");
        builder.Property(item => item.NearLeftDnp).HasColumnName("near_left_dnp").HasPrecision(5, 2);
        builder.Property(item => item.NearLeftHeight).HasColumnName("near_left_height").HasPrecision(5, 2);
        builder.Property(item => item.NearLeftAddition).HasColumnName("near_left_addition").HasPrecision(5, 2);
        builder.Ignore(item => item.UnitTotal);
        builder.Ignore(item => item.Total);
        builder.Ignore(item => item.Prescription);
        builder.HasIndex(item => item.SaleId).HasDatabaseName("ix_sale_items_sale_id");
        builder.HasIndex(item => item.FrameId).HasDatabaseName("ix_sale_items_frame_id");
        builder.HasOne(item => item.Frame).WithMany().HasForeignKey(item => item.FrameId).OnDelete(DeleteBehavior.Restrict);
    }
}
