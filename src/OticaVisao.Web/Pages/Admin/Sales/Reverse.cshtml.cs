using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class ReverseModel(SaleService saleService) : PageModel
{
    [BindProperty] public ReverseSaleInputModel Input { get; set; } = new();
    public SaleListItem Sale { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken)) return NotFound();
        if (Sale.Status != SaleStatus.Completed) return RedirectToPage("Details", new { id });
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }
        try
        {
            await saleService.ReverseAsync(id, Input.Reason, User.Identity?.Name ?? "Administrador", cancellationToken);
            TempData["SaleSuccessMessage"] = "Venda estornada, retirada dos relatórios e itens devolvidos ao estoque.";
            return RedirectToPage("Details", new { id });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }
    }

    private async Task<bool> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var sale = await saleService.GetAsync(id, cancellationToken);
        if (sale is null) return false;
        Sale = sale;
        return true;
    }
}
