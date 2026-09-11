using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class IndexModel(SaleService saleService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CustomerId { get; set; }

    public IReadOnlyList<SaleListItem> Sales { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var sales = await saleService.ListAsync(cancellationToken);
        if (CustomerId.HasValue) sales = sales.Where(sale => sale.CustomerId == CustomerId.Value).ToArray();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            sales = sales.Where(sale =>
                sale.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                sale.CustomerPhone.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                sale.Items.Any(item => item.FrameCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    item.FrameBrand.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    item.FrameModel.Contains(term, StringComparison.OrdinalIgnoreCase))).ToArray();
        }
        Sales = sales;
    }
}
