using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.LaboratoryOrders;

public sealed record LaboratoryOrderListItem(
    Guid Id, Guid SaleId, string CustomerName, string CustomerPhone,
    string FrameDescription, string LensDescription, int Quantity,
    OpticalLaboratory Laboratory, LaboratoryOrderStatus Status,
    DateOnly? ExpectedDeliveryDate, string? Notes,
    DateTimeOffset? SentAtUtc, DateTimeOffset? ReadyAtUtc, DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<LaboratoryOrderHistoryItem> History);

public sealed record LaboratoryOrderHistoryItem(LaboratoryOrderStatus Status, DateTimeOffset ChangedAtUtc);
