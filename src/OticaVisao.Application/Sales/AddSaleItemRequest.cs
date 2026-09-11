using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record AddSaleItemRequest(
    Guid FrameId,
    int Quantity,
    bool IncludesLenses,
    string? LensDescription,
    decimal LensUnitPrice,
    OpticalLaboratory? Laboratory,
    LensPrescription? Prescription = null,
    DateOnly? ExpectedDeliveryDate = null,
    decimal? FrameUnitPrice = null);
