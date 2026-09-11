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

    public async Task<IReadOnlyList<FrameCatalogItem>> ListPublicAsync(
        PublicFrameFilter filter,
        int? maximumItems = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        var frames = await repository.ListPublicAsync(filter, maximumItems, cancellationToken);
        return frames.Select(ToCatalogItem).ToArray();
    }

    public async Task<FrameCatalogItem?> GetPublicAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var frame = await repository.GetPublicByIdAsync(id, cancellationToken);
        return frame is null ? null : ToCatalogItem(frame);
    }

    public async Task<Guid> CreateAsync(
        CreateFrameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await repository.CodeExistsAsync(normalizedCode, cancellationToken: cancellationToken))
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
            request.Measurements,
            request.ImageFileName);

        await repository.AddAsync(frame, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return frame.Id;
    }

    public async Task<FrameCatalogItem?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var frame = await repository.GetByIdAsync(id, cancellationToken);
        return frame is null ? null : ToCatalogItem(frame);
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateFrameRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var frame = await GetRequiredAsync(id, cancellationToken);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();

        if (await repository.CodeExistsAsync(normalizedCode, id, cancellationToken))
        {
            throw new InvalidOperationException($"Já existe uma armação com o código '{normalizedCode}'.");
        }

        frame.UpdateCatalogDetails(
            normalizedCode,
            request.Brand,
            request.Model,
            request.Color,
            request.Type,
            request.Shape,
            request.TargetAudience,
            request.Measurements);
        frame.ChangePrice(request.Price);
        frame.SetStock(request.StockQuantity);

        if (request.IsActive)
        {
            frame.Reactivate();
            if (request.IsPublished)
            {
                frame.Publish();
            }
            else
            {
                frame.Unpublish();
            }
        }
        else
        {
            frame.Deactivate();
        }

        await repository.SaveChangesAsync(cancellationToken);
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

    public async Task RemoveFromStockAsync(
        Guid id,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var frame = await GetRequiredAsync(id, cancellationToken);
        frame.RemoveFromStock(quantity);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task SetImageAsync(
        Guid id,
        string imageFileName,
        CancellationToken cancellationToken = default)
    {
        var frame = await GetRequiredAsync(id, cancellationToken);
        frame.SetImage(imageFileName);
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
        frame.ReservedQuantity,
        frame.AvailableQuantity,
        frame.Type,
        frame.Shape,
        frame.TargetAudience,
        frame.Measurements?.LensWidthMillimeters,
        frame.Measurements?.BridgeWidthMillimeters,
        frame.Measurements?.TempleLengthMillimeters,
        frame.ImageFileName,
        frame.IsActive,
        frame.IsPublished,
        frame.IsAvailable);
}
