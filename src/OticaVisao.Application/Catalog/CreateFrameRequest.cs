using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public sealed record CreateFrameRequest(
    string Code,
    string Brand,
    string Model,
    string Color,
    decimal Price,
    int StockQuantity,
    FrameType Type,
    FrameShape Shape,
    TargetAudience TargetAudience,
    FrameMeasurements? Measurements = null,
    string? ImageFileName = null);
