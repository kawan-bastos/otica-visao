namespace OticaVisao.Application.Customers;

public sealed record CustomerListItem(
    Guid Id,
    string Name,
    string Phone,
    string? Email,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
