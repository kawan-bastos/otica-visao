using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Web.Pages.Frames;

public sealed class DetailsModel(FrameCatalogService catalogService) : PageModel
{
    public FrameCatalogItem Frame { get; private set; } = null!;

    public string WhatsAppUrl { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var frame = await catalogService.GetPublicAsync(id, cancellationToken);
        if (frame is null)
        {
            return NotFound();
        }

        Frame = frame;
        var message = $"Olá! Vi a armação {frame.Brand} {frame.Model}, código {frame.Code}, no site e gostaria de confirmar a disponibilidade.";
        WhatsAppUrl = $"https://wa.me/5521965912440?text={Uri.EscapeDataString(message)}";
        return Page();
    }

    public static string TypeName(FrameType type) => type == FrameType.Sunglasses
        ? "Óculos de sol"
        : "Óculos de grau";
}
