using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class EditModel(FrameCatalogService catalogService) : PageModel
{
    [BindProperty]
    public FrameInputModel Input { get; set; } = new();

    public Guid FrameId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var frame = await catalogService.GetAsync(id, cancellationToken);
        if (frame is null)
        {
            return NotFound();
        }

        FrameId = id;
        Input = FrameInputModel.FromCatalogItem(frame);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        FrameId = id;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await catalogService.UpdateAsync(id, Input.ToUpdateRequest(), cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Armação atualizada com sucesso.";
        return RedirectToPage("Index");
    }
}
