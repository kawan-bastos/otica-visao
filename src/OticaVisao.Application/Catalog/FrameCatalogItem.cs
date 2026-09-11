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
    int ReservedQuantity,
    int AvailableQuantity,
    FrameType Type,
    FrameShape Shape,
    TargetAudience TargetAudience,
    int? LensWidthMillimeters,
    int? BridgeWidthMillimeters,
    int? TempleLengthMillimeters,
    string? ImageFileName,
    bool IsActive,
    bool IsPublished,
    bool IsAvailable);
