using Microsoft.AspNetCore.Http;

namespace OticaVisao.Web.Images;

public static class FrameImageValidator
{
    public const long MaximumLength = 5 * 1024 * 1024;

    public static async Task<string> ValidateAsync(
        IFormFile image,
        CancellationToken cancellationToken = default)
    {
        if (image.Length is <= 0 or > MaximumLength)
        {
            throw new ArgumentException("A foto deve possuir no máximo 5 MB.");
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!Signatures.TryGetValue(extension, out var signatures))
        {
            throw new ArgumentException("Envie uma foto JPG, PNG ou WebP.");
        }

        await using var stream = image.OpenReadStream();
        var header = new byte[12];
        var bytesRead = await stream.ReadAsync(header, cancellationToken);

        var isValid = extension == ".webp"
            ? bytesRead >= 12
                && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && header.AsSpan(8, 4).SequenceEqual("WEBP"u8)
            : signatures.Any(signature =>
                bytesRead >= signature.Length
                && header.AsSpan(0, signature.Length).SequenceEqual(signature));

        if (!isValid)
        {
            throw new ArgumentException("O conteúdo do arquivo não corresponde a uma imagem válida.");
        }

        return extension;
    }

    private static readonly Dictionary<string, byte[][]> Signatures =
        new Dictionary<string, byte[][]>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = [[0xFF, 0xD8, 0xFF]],
            [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
            [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
            [".webp"] = []
        };
}
