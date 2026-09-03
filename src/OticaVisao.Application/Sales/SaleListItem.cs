using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record SaleListItem(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    SaleStatus Status,
    IReadOnlyList<SaleItemListItem> Items,
    decimal Total,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
