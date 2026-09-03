using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.LaboratoryOrders;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.LaboratoryOrders;

public sealed class DetailsModel(LaboratoryOrderService service) : PageModel
{
    [BindProperty]
    public LaboratoryOrderInputModel Input { get; set; } = new();
    public LaboratoryOrderListItem Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken)) return NotFound();
        Input.Status = Order.Status;
        Input.ExpectedDeliveryDate = Order.ExpectedDeliveryDate;
        Input.Notes = Order.Notes;
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
            await service.UpdateAsync(id, Input.Status, Input.ExpectedDeliveryDate, Input.Notes, cancellationToken);
            TempData["LaboratorySuccessMessage"] = "Acompanhamento atualizado.";
            return RedirectToPage(new { id });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            if (!await LoadAsync(id, cancellationToken)) return NotFound();
            return Page();
        }
    }

    private async Task<bool> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetAsync(id, cancellationToken);
        if (order is null) return false;
        Order = order;
        return true;
    }
}
