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

        public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Frames.SingleOrDefault(frame => frame.Id == id));

        public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(Frames.Any(frame => frame.Code == code));

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
