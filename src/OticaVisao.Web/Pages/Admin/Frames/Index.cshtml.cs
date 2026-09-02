using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class IndexModel(FrameCatalogService catalogService) : PageModel
{
    public IReadOnlyList<FrameCatalogItem> Frames { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Frames = await catalogService.ListAsync(cancellationToken);
    }
}
