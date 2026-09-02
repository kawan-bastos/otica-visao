using Microsoft.AspNetCore.Http;
using OticaVisao.Web.Images;

namespace OticaVisao.Tests.Web;

public sealed class FrameImageValidatorTests
{
    [Fact]
    public async Task ValidPngSignatureIsAccepted()
    {
        var image = FormFile(
            "armacao.png",
            [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00]);

        var extension = await FrameImageValidator.ValidateAsync(image);

        Assert.Equal(".png", extension);
    }

    [Fact]
    public async Task ExtensionThatDoesNotMatchContentIsRejected()
    {
        var image = FormFile("arquivo.png", "conteúdo que não é imagem"u8.ToArray());

        var action = () => FrameImageValidator.ValidateAsync(image);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    [Fact]
    public async Task ExecutableExtensionIsRejected()
    {
        var image = FormFile("foto.exe", [0xFF, 0xD8, 0xFF, 0x00]);

        var action = () => FrameImageValidator.ValidateAsync(image);

        await Assert.ThrowsAsync<ArgumentException>(action);
    }

    private static FormFile FormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "Image", fileName);
    }
}
