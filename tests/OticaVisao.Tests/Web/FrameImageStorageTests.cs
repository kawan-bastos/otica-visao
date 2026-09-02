using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using OticaVisao.Infrastructure;
using OticaVisao.Infrastructure.Images;

namespace OticaVisao.Tests.Web;

public sealed class FrameImageStorageTests : IDisposable
{
    private readonly string temporaryPath = Path.Combine(
        Path.GetTempPath(),
        $"otica-visao-images-{Guid.NewGuid():N}");

    [Fact]
    public async Task ConfiguredPathPersistsAndReadsImage()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment(temporaryPath));
        services.AddInfrastructure(
            "Host=localhost;Database=unused",
            options => options.Path = temporaryPath);

        await using var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredService<IFrameImageStorage>();
        var content = new byte[] { 1, 2, 3, 4 };

        await using var source = new MemoryStream(content);
        var fileName = await storage.SaveAsync(source, ".png");
        var stored = await storage.OpenReadAsync(fileName);

        Assert.NotNull(stored);
        Assert.Equal("image/png", stored.ContentType);
        await using var target = new MemoryStream();
        await stored.Content.CopyToAsync(target);
        await stored.Content.DisposeAsync();
        Assert.Equal(content, target.ToArray());

        await storage.DeleteAsync(fileName);
        Assert.Null(await storage.OpenReadAsync(fileName));
    }

    public void Dispose()
    {
        if (Directory.Exists(temporaryPath))
        {
            Directory.Delete(temporaryPath, recursive: true);
        }
    }

    private sealed class TestWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "OticaVisao.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
