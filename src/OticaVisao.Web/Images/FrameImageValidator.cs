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
        using var content = new MemoryStream((int)image.Length);
        await stream.CopyToAsync(content, cancellationToken);
        var bytes = content.ToArray();
        var header = bytes.AsSpan(0, Math.Min(bytes.Length, 12));
        var bytesRead = header.Length;

        var isValid = extension == ".webp"
            ? bytesRead >= 12
                && header[..4].SequenceEqual("RIFF"u8)
                && header.Slice(8, 4).SequenceEqual("WEBP"u8)
            : signatures.Any(signature =>
                bytesRead >= signature.Length
                && bytes.AsSpan(0, signature.Length).SequenceEqual(signature));

        if (!isValid)
        {
            throw new ArgumentException("O conteúdo do arquivo não corresponde a uma imagem válida.");
        }

        var dimensions = ReadDimensions(bytes, extension);
        if (dimensions is null || dimensions.Value.Width <= 0 || dimensions.Value.Height <= 0)
            throw new ArgumentException("Não foi possível validar as dimensões da imagem.");
        if ((long)dimensions.Value.Width * dimensions.Value.Height > 24_000_000)
            throw new ArgumentException("A imagem é grande demais. Use uma foto de até 24 megapixels.");

        return extension;
    }

    private static (int Width, int Height)? ReadDimensions(byte[] bytes, string extension) => extension switch
    {
        ".png" when bytes.Length >= 24 =>
            (ReadBigEndianInt32(bytes, 16), ReadBigEndianInt32(bytes, 20)),
        ".jpg" or ".jpeg" => ReadJpegDimensions(bytes),
        ".webp" => ReadWebpDimensions(bytes),
        _ => null
    };

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private static (int Width, int Height)? ReadJpegDimensions(byte[] bytes)
    {
        var offset = 2;
        while (offset + 8 < bytes.Length)
        {
            if (bytes[offset++] != 0xFF) continue;
            var marker = bytes[offset++];
            while (marker == 0xFF && offset < bytes.Length) marker = bytes[offset++];
            if (marker is 0xD8 or 0xD9) continue;
            if (offset + 1 >= bytes.Length) return null;
            var length = (bytes[offset] << 8) | bytes[offset + 1];
            if (length < 2 || offset + length > bytes.Length) return null;
            if (marker is 0xC0 or 0xC1 or 0xC2 or 0xC3 or 0xC5 or 0xC6 or 0xC7 or 0xC9 or 0xCA or 0xCB or 0xCD or 0xCE or 0xCF)
                return ((bytes[offset + 5] << 8) | bytes[offset + 6], (bytes[offset + 3] << 8) | bytes[offset + 4]);
            offset += length;
        }
        return null;
    }

    private static (int Width, int Height)? ReadWebpDimensions(byte[] bytes)
    {
        if (bytes.Length < 30) return null;
        var chunk = System.Text.Encoding.ASCII.GetString(bytes, 12, 4);
        if (chunk == "VP8X")
            return (1 + bytes[24] + (bytes[25] << 8) + (bytes[26] << 16),
                1 + bytes[27] + (bytes[28] << 8) + (bytes[29] << 16));
        if (chunk == "VP8L" && bytes.Length >= 25 && bytes[20] == 0x2F)
            return (1 + bytes[21] + ((bytes[22] & 0x3F) << 8),
                1 + (bytes[22] >> 6) + (bytes[23] << 2) + ((bytes[24] & 0x0F) << 10));
        if (chunk == "VP8 " && bytes.Length >= 30 && bytes[23] == 0x9D && bytes[24] == 0x01 && bytes[25] == 0x2A)
            return ((bytes[26] | (bytes[27] << 8)) & 0x3FFF, (bytes[28] | (bytes[29] << 8)) & 0x3FFF);
        return null;
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
