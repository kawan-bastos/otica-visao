namespace OticaVisao.Domain.LaboratoryOrders;

public enum LaboratoryOrderStatus
{
    AwaitingShipment = 1,
    Sent = 2,
    InProduction = 3,
    Ready = 4,
    Delivered = 5,
    Cancelled = 6
}
