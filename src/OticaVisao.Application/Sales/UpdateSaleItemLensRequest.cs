using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed record UpdateSaleItemLensRequest(
    string LensDescription,
    decimal LensUnitPrice,
    OpticalLaboratory Laboratory,
    LensPrescription Prescription,
    DateOnly? ExpectedDeliveryDate);
