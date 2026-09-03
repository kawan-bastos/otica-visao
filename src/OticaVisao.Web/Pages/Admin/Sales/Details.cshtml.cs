using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;
using OticaVisao.Application.Catalog;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class DetailsModel(SaleService saleService, FrameCatalogService frameCatalogService) : PageModel
{
    [BindProperty]
    public SaleItemInputModel Input { get; set; } = new();
    [BindProperty]
    public CompleteSaleInputModel Payment { get; set; } = new();
    public SaleListItem Sale { get; private set; } = null!;
    public IReadOnlyList<FrameCatalogItem> AvailableFrames { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken)) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAddItemAsync(Guid id, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        if (!TryValidateModel(Input, nameof(Input)))
        {
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }

        try
        {
            await saleService.AddItemAsync(id, Input.ToRequest(), cancellationToken);
            TempData["SaleSuccessMessage"] = "Item adicionado e estoque reservado.";
            return RedirectToPage(new { id });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveItemAsync(Guid id, Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            await saleService.RemoveItemAsync(id, itemId, cancellationToken);
            TempData["SaleSuccessMessage"] = "Item removido e quantidade devolvida ao estoque.";
            return RedirectToPage(new { id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        ModelState.Clear();
        if (!TryValidateModel(Payment, nameof(Payment)))
        {
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }

        try
        {
            await saleService.CompleteAsync(id, Payment.PaymentMethod!.Value, Payment.Installments, cancellationToken);
            TempData["SaleSuccessMessage"] = "Venda concluída com sucesso.";
            return RedirectToPage(new { id });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or KeyNotFoundException)
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
        AvailableFrames = (await frameCatalogService.ListAsync(cancellationToken))
            .Where(frame => frame.IsActive && frame.StockQuantity > 0 && Sale.Items.All(item => item.FrameId != frame.Id))
            .ToArray();
        return true;
    }
}
