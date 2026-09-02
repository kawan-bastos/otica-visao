using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Tests.Catalog;

public sealed class FrameCatalogServiceTests
{
    [Fact]
    public async Task CreateAddsFrameAndSavesChanges()
    {
        var repository = new FakeFrameRepository();
        var service = new FrameCatalogService(repository);

        var id = await service.CreateAsync(CreateRequest(" ARM-100 "));

        var frame = Assert.Single(repository.Frames);
        Assert.Equal(id, frame.Id);
        Assert.Equal("ARM-100", frame.Code);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task CreateRejectsDuplicatedCode()
    {
        var existing = CreateFrame("ARM-100");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(CreateRequest("arm-100")));

        Assert.Contains("ARM-100", exception.Message);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task ChangePriceUpdatesExistingFrame()
    {
        var existing = CreateFrame("ARM-101");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);

        await service.ChangePriceAsync(existing.Id, 199m);

        Assert.Equal(199m, existing.Price);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task RemoveFromStockUpdatesExistingFrame()
    {
        var existing = CreateFrame("ARM-106");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);

        await service.RemoveFromStockAsync(existing.Id, 1);

        Assert.Equal(1, existing.StockQuantity);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task RemoveFromStockRejectsQuantityAboveAvailableStock()
    {
        var existing = CreateFrame("ARM-107");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RemoveFromStockAsync(existing.Id, 3));

        Assert.Equal(2, existing.StockQuantity);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task UpdateChangesCatalogDataAndVisibility()
    {
        var existing = CreateFrame("ARM-101");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);
        var request = new UpdateFrameRequest(
            " arm-102 ",
            "Nova marca",
            "Novo modelo",
            "Azul",
            199m,
            4,
            FrameType.Sunglasses,
            FrameShape.Round,
            TargetAudience.Child,
            true,
            true,
            new FrameMeasurements(50, 18, 140));

        await service.UpdateAsync(existing.Id, request);

        Assert.Equal("ARM-102", existing.Code);
        Assert.Equal(199m, existing.Price);
        Assert.Equal(4, existing.StockQuantity);
        Assert.True(existing.IsPublished);
        Assert.True(existing.IsAvailable);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task SetImageUpdatesStoredFileReference()
    {
        var existing = CreateFrame("ARM-103");
        var repository = new FakeFrameRepository(existing);
        var service = new FrameCatalogService(repository);

        await service.SetImageAsync(existing.Id, "imagem-segura.webp");

        Assert.Equal("imagem-segura.webp", existing.ImageFileName);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task PublicCatalogExcludesUnpublishedFrames()
    {
        var visible = CreateFrame("ARM-104");
        visible.Publish();
        var hidden = CreateFrame("ARM-105");
        var repository = new FakeFrameRepository(visible, hidden);
        var service = new FrameCatalogService(repository);

        var frames = await service.ListPublicAsync(new PublicFrameFilter());

        var frame = Assert.Single(frames);
        Assert.Equal("ARM-104", frame.Code);
    }

    private static CreateFrameRequest CreateRequest(string code) => new(
        code,
        "Linha Visão",
        "Clássica",
        "Preto",
        219m,
        2,
        FrameType.Prescription,
        FrameShape.Square,
        TargetAudience.Adult);

    private static Frame CreateFrame(string code) => new(
        code,
        "Linha Visão",
        "Clássica",
        "Preto",
        219m,
        2,
        FrameType.Prescription,
        FrameShape.Square,
        TargetAudience.Adult);

    private sealed class FakeFrameRepository(params Frame[] frames) : IFrameRepository
    {
        public List<Frame> Frames { get; } = [.. frames];

        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Frame>>(Frames);

        public Task<IReadOnlyList<Frame>> ListPublicAsync(
            PublicFrameFilter filter,
            int? maximumItems = null,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Frame> query = Frames.Where(frame => frame.IsAvailable);
            if (filter.Type.HasValue)
            {
                query = query.Where(frame => frame.Type == filter.Type);
            }

            if (maximumItems.HasValue)
            {
                query = query.Take(maximumItems.Value);
            }

            return Task.FromResult<IReadOnlyList<Frame>>(query.ToArray());
        }

        public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Frames.SingleOrDefault(frame => frame.Id == id));

        public Task<Frame?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Frames.SingleOrDefault(frame => frame.Id == id && frame.IsAvailable));

        public Task<bool> CodeExistsAsync(
            string code,
            Guid? excludingId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Frames.Any(frame => frame.Code == code && frame.Id != excludingId));

        public Task AddAsync(Frame frame, CancellationToken cancellationToken = default)
        {
            Frames.Add(frame);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
