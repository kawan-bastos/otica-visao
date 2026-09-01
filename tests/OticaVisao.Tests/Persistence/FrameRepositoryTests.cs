using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Catalog;
using OticaVisao.Infrastructure.Catalog;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Tests.Persistence;

public sealed class FrameRepositoryTests
{
    [Fact]
    public async Task RepositoryPersistsAndListsFramesInCatalogOrder()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"catalog-{Guid.NewGuid()}")
            .Options;

        await using var context = new ApplicationDbContext(options);
        var repository = new FrameRepository(context);

        await repository.AddAsync(CreateFrame("ARM-002", "Visão", "Zênite"));
        await repository.AddAsync(CreateFrame("ARM-001", "Alfa", "Clássica"));
        await repository.SaveChangesAsync();

        var frames = await repository.ListAsync();

        Assert.Collection(
            frames,
            first => Assert.Equal("ARM-001", first.Code),
            second => Assert.Equal("ARM-002", second.Code));
        Assert.True(await repository.CodeExistsAsync("ARM-002"));
    }

    private static Frame CreateFrame(string code, string brand, string model) => new(
        code,
        brand,
        model,
        "Preto",
        219m,
        1,
        FrameType.Prescription,
        FrameShape.Square,
        TargetAudience.Adult);
}
