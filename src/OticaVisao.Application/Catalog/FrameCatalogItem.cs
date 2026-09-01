using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public sealed record FrameCatalogItem(
    Guid Id,
    string Code,
    string Brand,
    string Model,
    string Color,
    decimal Price,
    int StockQuantity,
    FrameType Type,
    FrameShape Shape,
    TargetAudience TargetAudience,
    bool IsActive,
    bool IsPublished,
    bool IsAvailable);
