namespace OticaVisao.Domain.Catalog;

public sealed class Frame
{
    private Frame()
    {
        Code = null!;
        Brand = null!;
        Model = null!;
        Color = null!;
    }

    public Frame(
        string code,
        string brand,
        string model,
        string color,
        decimal price,
        int stockQuantity,
        FrameType type,
        FrameShape shape,
        TargetAudience targetAudience,
        FrameMeasurements? measurements = null)
    {
        Id = Guid.NewGuid();
        Code = RequiredText(code, nameof(code), 40);
        Brand = RequiredText(brand, nameof(brand), 100);
        Model = RequiredText(model, nameof(model), 100);
        Color = RequiredText(color, nameof(color), 80);
        Price = ValidPrice(price);
        StockQuantity = ValidStock(stockQuantity);
        Type = type;
        Shape = shape;
        TargetAudience = targetAudience;
        Measurements = measurements;
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; }

    public string Brand { get; private set; }

    public string Model { get; private set; }

    public string Color { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    public FrameType Type { get; private set; }

    public FrameShape Shape { get; private set; }

    public TargetAudience TargetAudience { get; private set; }

    public FrameMeasurements? Measurements { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsPublished { get; private set; }

    public bool IsAvailable => IsActive && IsPublished && StockQuantity > 0;

    public void Publish()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Uma armação desativada não pode ser publicada.");
        }

        IsPublished = true;
    }

    public void Unpublish() => IsPublished = false;

    public void Deactivate()
    {
        IsActive = false;
        IsPublished = false;
    }

    public void Reactivate() => IsActive = true;

    public void AddToStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "A quantidade adicionada deve ser maior que zero.");
        }

        StockQuantity = checked(StockQuantity + quantity);
    }

    public void RemoveFromStock(int quantity = 1)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "A quantidade removida deve ser maior que zero.");
        }

        if (quantity > StockQuantity)
        {
            throw new InvalidOperationException("Não há estoque suficiente para realizar a baixa.");
        }

        StockQuantity -= quantity;
    }

    public void ChangePrice(decimal price) => Price = ValidPrice(price);

    public void SetStock(int stockQuantity) => StockQuantity = ValidStock(stockQuantity);

    public void UpdateCatalogDetails(
        string code,
        string brand,
        string model,
        string color,
        FrameType type,
        FrameShape shape,
        TargetAudience targetAudience,
        FrameMeasurements? measurements = null)
    {
        Code = RequiredText(code, nameof(code), 40);
        Brand = RequiredText(brand, nameof(brand), 100);
        Model = RequiredText(model, nameof(model), 100);
        Color = RequiredText(color, nameof(color), 80);
        Type = type;
        Shape = shape;
        TargetAudience = targetAudience;
        Measurements = measurements;
    }

    private static string RequiredText(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);

        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"O campo deve possuir no máximo {maximumLength} caracteres.", parameterName);
        }

        return normalized;
    }

    private static decimal ValidPrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, "O preço não pode ser negativo.");
        }

        return price;
    }

    private static int ValidStock(int stockQuantity)
    {
        if (stockQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stockQuantity), stockQuantity, "O estoque não pode ser negativo.");
        }

        return stockQuantity;
    }
}
