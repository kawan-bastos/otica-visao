using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class CreateModel(FrameCatalogService catalogService) : PageModel
{
    [BindProperty]
    public FrameInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await catalogService.CreateAsync(Input.ToCreateRequest(), cancellationToken);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Armação cadastrada com sucesso.";
        return RedirectToPage("Index");
    }
}
