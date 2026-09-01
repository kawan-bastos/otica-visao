using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public sealed class FrameCatalogService(IFrameRepository repository)
{
    public async Task<IReadOnlyList<FrameCatalogItem>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var frames = await repository.ListAsync(cancellationToken);
        return frames.Select(ToCatalogItem).ToArray();
    }

    public async Task<Guid> CreateAsync(
        CreateFrameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(normalizedCode, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe uma armação com o código '{normalizedCode}'.");
        }

        var frame = new Frame(
            normalizedCode,
            request.Brand,
            request.Model,
            request.Color,
            request.Price,
            request.StockQuantity,
            request.Type,
            request.Shape,
            request.TargetAudience,
            request.Measurements);

        await repository.AddAsync(frame, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return frame.Id;
    }

    public async Task ChangePriceAsync(
        Guid id,
        decimal price,
        CancellationToken cancellationToken = default)
    {
        var frame = await GetRequiredAsync(id, cancellationToken);
        frame.ChangePrice(price);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToStockAsync(
        Guid id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var frame = await GetRequiredAsync(id, cancellationToken);
        frame.AddToStock(quantity);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<Frame> GetRequiredAsync(Guid id, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Armação não encontrada.");
    }

    private static FrameCatalogItem ToCatalogItem(Frame frame) => new(
        frame.Id,
        frame.Code,
        frame.Brand,
        frame.Model,
        frame.Color,
        frame.Price,
        frame.StockQuantity,
        frame.Type,
        frame.Shape,
        frame.TargetAudience,
        frame.IsActive,
        frame.IsPublished,
        frame.IsAvailable);
}
