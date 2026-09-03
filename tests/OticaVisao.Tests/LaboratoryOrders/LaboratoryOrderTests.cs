using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Tests.LaboratoryOrders;

public sealed class LaboratoryOrderTests
{
    [Fact]
    public void NewOrderStartsAwaitingShipmentWithHistory()
    {
        var (sale, item) = CompletedSaleWithLenses();

        var order = new LaboratoryOrder(sale, item);

        Assert.Equal(LaboratoryOrderStatus.AwaitingShipment, order.Status);
        Assert.Equal(OpticalLaboratory.ImperialLab, order.Laboratory);
        Assert.Equal(LaboratoryOrderStatus.AwaitingShipment, Assert.Single(order.History).Status);
    }

    [Fact]
    public void UpdatingStatusRecordsDatesNotesAndHistory()
    {
        var (sale, item) = CompletedSaleWithLenses();
        var order = new LaboratoryOrder(sale, item);
        var expectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

        order.Update(LaboratoryOrderStatus.Ready, expectedDate, " Pedido 123 ");

        Assert.Equal(LaboratoryOrderStatus.Ready, order.Status);
        Assert.Equal(expectedDate, order.ExpectedDeliveryDate);
        Assert.Equal("Pedido 123", order.Notes);
        Assert.NotNull(order.SentAtUtc);
        Assert.NotNull(order.ReadyAtUtc);
        Assert.Equal(2, order.History.Count);
    }

    [Fact]
    public void DeliveredOrderCannotReturnToPreviousStatus()
    {
        var (sale, item) = CompletedSaleWithLenses();
        var order = new LaboratoryOrder(sale, item);
        order.Update(LaboratoryOrderStatus.Delivered, null, null);

        Assert.Throws<InvalidOperationException>(() => order.Update(LaboratoryOrderStatus.Ready, null, null));
    }

    private static (Sale Sale, SaleItem Item) CompletedSaleWithLenses()
    {
        var sale = new Sale(Guid.NewGuid());
        var frame = new Frame("LAB-1", "Marca", "Modelo", "Preta", 219m, 1,
            FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);
        var item = sale.AddItem(frame, 1, true, "Visão simples", 300m, OpticalLaboratory.ImperialLab);
        sale.Complete(PaymentMethod.Pix, 1);
        return (sale, item);
    }
}
