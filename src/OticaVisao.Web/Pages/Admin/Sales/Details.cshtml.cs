using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class DetailsModel(SaleService saleService) : PageModel
{
    public SaleListItem Sale { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await saleService.GetAsync(id, cancellationToken);
        if (sale is null) return NotFound();
        Sale = sale;
        return Page();
    }
}
