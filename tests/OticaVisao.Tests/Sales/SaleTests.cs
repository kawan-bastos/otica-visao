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
    public void PrescriptionIsStoredAndCanBeCorrectedAfterCompletion()
    {
        var sale = new Sale(Guid.NewGuid());
        var original = new LensPrescription(new(-2m,-0.5m,90,31m,18m,null), PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty);
        var item = sale.AddItem(Frame(), 1, true, "Visão simples", 300m, OpticalLaboratory.ImperialLab, original);
        sale.Complete(PaymentMethod.Pix, 1);

        var corrected = new LensPrescription(new(-2.25m,-0.5m,90,31m,18m,null), PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty);
        sale.UpdateItemLensDetails(item.Id, "Visão simples", 310m, OpticalLaboratory.StandardOptical, corrected);

        Assert.Equal(-2.25m, item.Prescription.FarRight.Sphere);
        Assert.Equal(OpticalLaboratory.StandardOptical, item.Laboratory);
        Assert.Equal(349m, sale.FinalTotal);
    }

    [Fact]
    public void PrescriptionRejectsInvalidAxis()
    {
        var prescription = new LensPrescription(new(null,null,181,null,null,null), PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty);
        Assert.Throws<ArgumentOutOfRangeException>(() => prescription.Validate());
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

    [Fact]
    public void CompleteSaleRecordsPaymentAndLocksChanges()
    {
        var sale = SaleWithItem();

        sale.Complete(PaymentMethod.CreditCard, 10);

        Assert.Equal(SaleStatus.Completed, sale.Status);
        Assert.Equal(PaymentMethod.CreditCard, sale.PaymentMethod);
        Assert.Equal(10, sale.Installments);
        Assert.Equal(219m, sale.FinalTotal);
        Assert.NotNull(sale.CompletedAtUtc);
        Assert.Throws<InvalidOperationException>(() => sale.AddItem(Frame(), 1, false, null, 0, null));
        Assert.Throws<InvalidOperationException>(sale.Cancel);
    }

    [Fact]
    public void CompleteSaleRequiresItemsAndValidInstallments()
    {
        var emptySale = new Sale(Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => emptySale.Complete(PaymentMethod.Pix, 1));

        var sale = SaleWithItem();
        Assert.Throws<ArgumentException>(() => sale.Complete(PaymentMethod.CreditCard, 11));
        Assert.Throws<ArgumentException>(() => sale.Complete(PaymentMethod.DebitCard, 2));
    }

    [Fact]
    public void CancelSaleRecordsStatusAndLocksChanges()
    {
        var sale = SaleWithItem();

        sale.Cancel();

        Assert.Equal(SaleStatus.Cancelled, sale.Status);
        Assert.NotNull(sale.CancelledAtUtc);
        Assert.Throws<InvalidOperationException>(() => sale.Complete(PaymentMethod.Pix, 1));
    }

    [Fact]
    public void ReverseSaleRecordsReasonAndDate()
    {
        var sale = SaleWithItem();
        sale.Complete(PaymentMethod.Pix, 1);

        sale.Reverse(" Venda criada para teste ", "Carlos");

        Assert.Equal(SaleStatus.Reversed, sale.Status);
        Assert.Equal("Venda criada para teste", sale.ReversalReason);
        Assert.Equal("Carlos", sale.ReversedBy);
        Assert.NotNull(sale.ReversedAtUtc);
        Assert.Throws<InvalidOperationException>(() => sale.Reverse("Novamente", "Carlos"));
    }

    private static Sale SaleWithItem()
    {
        var sale = new Sale(Guid.NewGuid());
        sale.AddItem(Frame(), 1, false, null, 0, null);
        return sale;
    }

    private static Frame Frame() => new(
        "ARM-TEST", "Marca", "Modelo", "Preta", 219m, 3,
        FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);
}
