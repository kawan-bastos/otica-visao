using OticaVisao.Domain.Catalog;

namespace OticaVisao.Domain.Sales;

public sealed class SaleItem
{
    public const decimal CompleteGlassesFramePrice = 39m;

    private SaleItem() { }

    internal SaleItem(Frame frame, int quantity, bool includesLenses, string? lensDescription, decimal lensUnitPrice, OpticalLaboratory? laboratory, LensPrescription? prescription = null, DateOnly? expectedDeliveryDate = null, decimal? frameUnitPrice = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero.");
        if (lensUnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(lensUnitPrice), "O preço das lentes não pode ser negativo.");
        if (frameUnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(frameUnitPrice), "O preço da armação não pode ser negativo.");
        if (includesLenses && !laboratory.HasValue) throw new ArgumentException("Selecione o laboratório das lentes.", nameof(laboratory));
        if (includesLenses && string.IsNullOrWhiteSpace(lensDescription)) throw new ArgumentException("Descreva as lentes.", nameof(lensDescription));
        if (!includesLenses && (laboratory.HasValue || lensUnitPrice != 0 || !string.IsNullOrWhiteSpace(lensDescription)))
            throw new ArgumentException("Dados de lentes só podem ser informados no óculos completo.");

        Id = Guid.NewGuid();
        FrameId = frame.Id;
        FrameCode = frame.Code;
        FrameBrand = frame.Brand;
        FrameModel = frame.Model;
        FrameColor = frame.Color;
        Quantity = quantity;
        IncludesLenses = includesLenses;
        FrameUnitPrice = frameUnitPrice ?? (includesLenses ? CompleteGlassesFramePrice : frame.Price);
        LensDescription = NormalizeOptional(lensDescription, 500);
        LensUnitPrice = lensUnitPrice;
        Laboratory = laboratory;
        ExpectedDeliveryDate = includesLenses ? expectedDeliveryDate : null;
        SetPrescription(includesLenses ? prescription ?? LensPrescription.Empty : LensPrescription.Empty);
    }

    public Guid Id { get; private set; }
    public Guid SaleId { get; private set; }
    public Guid FrameId { get; private set; }
    public Frame Frame { get; private set; } = null!;
    public string FrameCode { get; private set; } = null!;
    public string FrameBrand { get; private set; } = null!;
    public string FrameModel { get; private set; } = null!;
    public string FrameColor { get; private set; } = null!;
    public int Quantity { get; private set; }
    public bool IncludesLenses { get; private set; }
    public decimal FrameUnitPrice { get; private set; }
    public string? LensDescription { get; private set; }
    public decimal LensUnitPrice { get; private set; }
    public OpticalLaboratory? Laboratory { get; private set; }
    public DateOnly? ExpectedDeliveryDate { get; private set; }
    public decimal? FarRightSphere { get; private set; }
    public decimal? FarRightCylinder { get; private set; }
    public int? FarRightAxis { get; private set; }
    public decimal? FarRightDnp { get; private set; }
    public decimal? FarRightHeight { get; private set; }
    public decimal? FarRightAddition { get; private set; }
    public decimal? FarLeftSphere { get; private set; }
    public decimal? FarLeftCylinder { get; private set; }
    public int? FarLeftAxis { get; private set; }
    public decimal? FarLeftDnp { get; private set; }
    public decimal? FarLeftHeight { get; private set; }
    public decimal? FarLeftAddition { get; private set; }
    public decimal? NearRightSphere { get; private set; }
    public decimal? NearRightCylinder { get; private set; }
    public int? NearRightAxis { get; private set; }
    public decimal? NearRightDnp { get; private set; }
    public decimal? NearRightHeight { get; private set; }
    public decimal? NearRightAddition { get; private set; }
    public decimal? NearLeftSphere { get; private set; }
    public decimal? NearLeftCylinder { get; private set; }
    public int? NearLeftAxis { get; private set; }
    public decimal? NearLeftDnp { get; private set; }
    public decimal? NearLeftHeight { get; private set; }
    public decimal? NearLeftAddition { get; private set; }
    public LensPrescription Prescription => new(
        new(FarRightSphere, FarRightCylinder, FarRightAxis, FarRightDnp, FarRightHeight, FarRightAddition),
        new(FarLeftSphere, FarLeftCylinder, FarLeftAxis, FarLeftDnp, FarLeftHeight, FarLeftAddition),
        new(NearRightSphere, NearRightCylinder, NearRightAxis, NearRightDnp, NearRightHeight, NearRightAddition),
        new(NearLeftSphere, NearLeftCylinder, NearLeftAxis, NearLeftDnp, NearLeftHeight, NearLeftAddition));
    public decimal UnitTotal => FrameUnitPrice + LensUnitPrice;
    public decimal Total => UnitTotal * Quantity;

    internal void UpdateLensDetails(string lensDescription, decimal lensUnitPrice, OpticalLaboratory laboratory, LensPrescription prescription, DateOnly? expectedDeliveryDate)
    {
        if (!IncludesLenses) throw new InvalidOperationException("Este item não possui lentes.");
        if (lensUnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(lensUnitPrice), "O preço das lentes não pode ser negativo.");
        LensDescription = NormalizeOptional(lensDescription, 500)
            ?? throw new ArgumentException("Descreva as lentes.", nameof(lensDescription));
        LensUnitPrice = lensUnitPrice;
        Laboratory = laboratory;
        ExpectedDeliveryDate = expectedDeliveryDate;
        SetPrescription(prescription);
    }

    private void SetPrescription(LensPrescription prescription)
    {
        prescription.Validate();
        (FarRightSphere, FarRightCylinder, FarRightAxis, FarRightDnp, FarRightHeight, FarRightAddition) = prescription.FarRight;
        (FarLeftSphere, FarLeftCylinder, FarLeftAxis, FarLeftDnp, FarLeftHeight, FarLeftAddition) = prescription.FarLeft;
        (NearRightSphere, NearRightCylinder, NearRightAxis, NearRightDnp, NearRightHeight, NearRightAddition) = prescription.NearRight;
        (NearLeftSphere, NearLeftCylinder, NearLeftAxis, NearLeftDnp, NearLeftHeight, NearLeftAddition) = prescription.NearLeft;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"A descrição deve possuir no máximo {maximumLength} caracteres.", nameof(value));
        return normalized;
    }
}
