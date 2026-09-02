using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace OticaVisao.Infrastructure.Images;

internal sealed class LocalFrameImageStorage(
    IWebHostEnvironment environment,
    IOptions<FrameImageStorageOptions> options) : IFrameImageStorage
{
    private readonly string storagePath = ResolveStoragePath(environment, options.Value.Path);

    public async Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        var normalizedExtension = extension.ToLowerInvariant();
        if (!ContentTypes.ContainsKey(normalizedExtension))
        {
            throw new ArgumentException("Formato de imagem não permitido.", nameof(extension));
        }

        Directory.CreateDirectory(storagePath);
        var fileName = $"{Guid.NewGuid():N}{normalizedExtension}";
        var path = Path.Combine(storagePath, fileName);

        await using var target = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(target, cancellationToken);
        return fileName;
    }

    public Task<StoredFrameImage?> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!IsSafeFileName(fileName))
        {
            return Task.FromResult<StoredFrameImage?>(null);
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var path = Path.Combine(storagePath, fileName);
        if (!ContentTypes.TryGetValue(extension, out var contentType) || !File.Exists(path))
        {
            return Task.FromResult<StoredFrameImage?>(null);
        }

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<StoredFrameImage?>(new StoredFrameImage(stream, contentType));
    }

    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        if (IsSafeFileName(fileName))
        {
            var path = Path.Combine(storagePath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsSafeFileName(string fileName) =>
        !string.IsNullOrWhiteSpace(fileName)
        && fileName == Path.GetFileName(fileName)
        && fileName.Length <= 80;

    private static string ResolveStoragePath(IWebHostEnvironment environment, string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.Combine(environment.ContentRootPath, "App_Data", "frame-images");
        }

        return Path.GetFullPath(
            Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(environment.ContentRootPath, configuredPath));
    }

    private static readonly Dictionary<string, string> ContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };
}
