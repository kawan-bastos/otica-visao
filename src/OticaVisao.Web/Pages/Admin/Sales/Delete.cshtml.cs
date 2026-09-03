using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class DeleteModel(SaleService saleService) : PageModel
{
    public SaleListItem Sale { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await saleService.GetAsync(id, cancellationToken);
        if (sale is null) return NotFound();
        if (sale.Status is not (SaleStatus.Cancelled or SaleStatus.Reversed)) return RedirectToPage("Details", new { id });
        Sale = sale;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var sale = await saleService.GetAsync(id, cancellationToken);
            if (sale is null) return NotFound();
            if (sale.Status == SaleStatus.Reversed) await saleService.DeleteReversedAsync(id, cancellationToken);
            else await saleService.DeleteCancelledAsync(id, cancellationToken);
            TempData["SaleSuccessMessage"] = "Venda excluída permanentemente do histórico.";
            return RedirectToPage("Index");
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException) { return RedirectToPage("Details", new { id }); }
    }
}
