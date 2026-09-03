using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record SaleItemListItem(
    Guid Id,
    Guid FrameId,
    string FrameCode,
    string FrameBrand,
    string FrameModel,
    string FrameColor,
    int Quantity,
    bool IncludesLenses,
    decimal FrameUnitPrice,
    string? LensDescription,
    decimal LensUnitPrice,
    OpticalLaboratory? Laboratory,
    LensPrescription Prescription,
    DateOnly? ExpectedDeliveryDate,
    decimal Total);
