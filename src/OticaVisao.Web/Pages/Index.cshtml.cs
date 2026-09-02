using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;

namespace OticaVisao.Web.Pages;

public class IndexModel(FrameCatalogService catalogService) : PageModel
{
    public IReadOnlyList<FrameCatalogItem> FeaturedFrames { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        FeaturedFrames = await catalogService.ListPublicAsync(
            new PublicFrameFilter(),
            maximumItems: 4,
            cancellationToken);
    }
}
