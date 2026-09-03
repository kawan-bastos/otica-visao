using OticaVisao.Domain.Catalog;

namespace OticaVisao.Domain.Sales;

public sealed class SaleItem
{
    public const decimal CompleteGlassesFramePrice = 39m;

    private SaleItem() { }

    internal SaleItem(Frame frame, int quantity, bool includesLenses, string? lensDescription, decimal lensUnitPrice, OpticalLaboratory? laboratory)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade deve ser maior que zero.");
        if (lensUnitPrice < 0) throw new ArgumentOutOfRangeException(nameof(lensUnitPrice), "O preço das lentes não pode ser negativo.");
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
        FrameUnitPrice = includesLenses ? CompleteGlassesFramePrice : frame.Price;
        LensDescription = NormalizeOptional(lensDescription, 500);
        LensUnitPrice = lensUnitPrice;
        Laboratory = laboratory;
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
    public decimal UnitTotal => FrameUnitPrice + LensUnitPrice;
    public decimal Total => UnitTotal * Quantity;

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"A descrição deve possuir no máximo {maximumLength} caracteres.", nameof(value));
        return normalized;
    }
}
