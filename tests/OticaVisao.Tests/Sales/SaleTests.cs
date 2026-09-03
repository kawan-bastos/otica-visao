using OticaVisao.Domain.Sales;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Tests.Sales;

public sealed class SaleTests
{
    [Fact]
    public void NewSaleStartsAsDraft()
    {
        var customerId = Guid.NewGuid();

        var sale = new Sale(customerId);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(customerId, sale.CustomerId);
        Assert.Equal(SaleStatus.Draft, sale.Status);
        Assert.Equal(sale.CreatedAtUtc, sale.UpdatedAtUtc);
    }

    [Fact]
    public void SaleRequiresCustomer() =>
        Assert.Throws<ArgumentException>(() => new Sale(Guid.Empty));

    [Fact]
    public void CompleteGlassesUsesPromotionalFramePrice()
    {
        var sale = new Sale(Guid.NewGuid());
        var frame = new Frame(
            "ARM-1", "Marca", "Modelo", "Preta", 219m, 1,
            FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);

        var item = sale.AddItem(
            frame, 1, true, "Visão simples", 300m, OpticalLaboratory.ImperialLab);

        Assert.Equal(39m, item.FrameUnitPrice);
        Assert.Equal(339m, item.Total);
        Assert.Equal(339m, sale.Total);
    }

    [Fact]
    public void FrameWithoutLensesUsesCatalogPrice()
    {
        var sale = new Sale(Guid.NewGuid());
        var frame = new Frame(
            "SOL-1", "Marca", "Modelo", "Preta", 219m, 1,
            FrameType.Sunglasses, FrameShape.Round, TargetAudience.Adult);

        var item = sale.AddItem(frame, 1, false, null, 0, null);

        Assert.Equal(219m, item.FrameUnitPrice);
        Assert.Equal(219m, sale.Total);
    }
}
