using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Web.Pages.Frames;

public sealed class IndexModel(FrameCatalogService catalogService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public FrameType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public FrameShape? Shape { get; set; }

    [BindProperty(SupportsGet = true)]
    public TargetAudience? TargetAudience { get; set; }

    public IReadOnlyList<FrameCatalogItem> Frames { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Frames = await catalogService.ListPublicAsync(
            new PublicFrameFilter(Search, Type, Shape, TargetAudience),
            cancellationToken: cancellationToken);
    }

    public static string TypeName(FrameType type) => type == FrameType.Sunglasses
        ? "Óculos de sol"
        : "Óculos de grau";
}
