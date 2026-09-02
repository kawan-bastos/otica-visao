using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Web.Models.Admin;
using OticaVisao.Web.Images;
using OticaVisao.Infrastructure.Images;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class CreateModel(
    FrameCatalogService catalogService,
    IFrameImageStorage imageStorage) : PageModel
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

        if (Input.Image is null)
        {
            ModelState.AddModelError("Input.Image", "Selecione a foto principal da armação.");
            return Page();
        }

        string extension;
        try
        {
            extension = await FrameImageValidator.ValidateAsync(Input.Image, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError("Input.Image", exception.Message);
            return Page();
        }

        string? savedFileName = null;

        try
        {
            await using var content = Input.Image.OpenReadStream();
            savedFileName = await imageStorage.SaveAsync(content, extension, cancellationToken);
            await catalogService.CreateAsync(Input.ToCreateRequest(savedFileName), cancellationToken);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            if (savedFileName is not null)
            {
                await imageStorage.DeleteAsync(savedFileName, cancellationToken);
            }

            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }

        TempData["SuccessMessage"] = "Armação cadastrada com sucesso.";
        return RedirectToPage("Index");
    }
}
