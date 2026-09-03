using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.LaboratoryOrders;
using OticaVisao.Web.Models.Admin;
using OticaVisao.Infrastructure.Images;
using OticaVisao.Web.Images;

namespace OticaVisao.Web.Pages.Admin.LaboratoryOrders;

public sealed class DetailsModel(LaboratoryOrderService service, ILaboratoryDocumentStorage storage) : PageModel
{
    [BindProperty]
    public LaboratoryOrderInputModel Input { get; set; } = new();
    public LaboratoryOrderListItem Order { get; private set; } = null!;
    [BindProperty] public IFormFile? Document { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken)) return NotFound();
        Input.Status = Order.Status;
        Input.Laboratory = Order.Laboratory;
        Input.ExpectedDeliveryDate = Order.ExpectedDeliveryDate;
        Input.Notes = Order.Notes;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await LoadAsync(id, cancellationToken)) return NotFound();
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            string? newFile = null;
            if (Document is not null) { var extension = await FrameImageValidator.ValidateAsync(Document, cancellationToken); await using var stream = Document.OpenReadStream(); newFile = await storage.SaveAsync(stream, extension, cancellationToken); }
            var oldFile = Order.DocumentFileName;
            try { await service.UpdateAsync(id, Input.Laboratory!.Value, Input.Status, Input.ExpectedDeliveryDate, Input.Notes, newFile, cancellationToken); }
            catch { if (newFile is not null) await storage.DeleteAsync(newFile, cancellationToken); throw; }
            if (newFile is not null && oldFile is not null) await storage.DeleteAsync(oldFile, cancellationToken);
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

    public async Task<IActionResult> OnGetDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetAsync(id, cancellationToken);
        if (order?.DocumentFileName is null) return NotFound();
        var document = await storage.OpenReadAsync(order.DocumentFileName, cancellationToken);
        return document is null ? NotFound() : File(document.Content, document.ContentType);
    }

    private async Task<bool> LoadAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetAsync(id, cancellationToken);
        if (order is null) return false;
        Order = order;
        return true;
    }
}
