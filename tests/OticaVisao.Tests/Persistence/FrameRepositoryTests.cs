using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Catalog;
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
        Assert.False(await repository.CodeExistsAsync("ARM-002", frames[1].Id));
    }

    [Fact]
    public async Task PublicQueryReturnsOnlyPublishedActiveFramesWithStock()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"public-catalog-{Guid.NewGuid()}")
            .Options;
        await using var context = new ApplicationDbContext(options);
        var repository = new FrameRepository(context);
        var available = CreateFrame("ARM-010", "Visão", "Disponível");
        available.Publish();
        var unpublished = CreateFrame("ARM-011", "Visão", "Oculta");
        await repository.AddAsync(available);
        await repository.AddAsync(unpublished);
        await repository.SaveChangesAsync();

        var frames = await repository.ListPublicAsync(new PublicFrameFilter());

        var frame = Assert.Single(frames);
        Assert.Equal("ARM-010", frame.Code);
        Assert.NotNull(await repository.GetPublicByIdAsync(available.Id));
        Assert.Null(await repository.GetPublicByIdAsync(unpublished.Id));
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
