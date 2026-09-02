namespace OticaVisao.Infrastructure.Images;

public interface IFrameImageStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);

    Task<StoredFrameImage?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}

public sealed record StoredFrameImage(Stream Content, string ContentType);
