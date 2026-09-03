using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record SaleListItem(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    SaleStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
