using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class IndexModel(SaleService saleService) : PageModel
{
    public IReadOnlyList<SaleListItem> Sales { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Sales = await saleService.ListAsync(cancellationToken);
}
