using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Web.Models.Admin;
using OticaVisao.Web.Images;
using OticaVisao.Infrastructure.Images;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class EditModel(
    FrameCatalogService catalogService,
    IFrameImageStorage imageStorage) : PageModel
{
    [BindProperty]
    public FrameInputModel Input { get; set; } = new();

    public Guid FrameId { get; private set; }

    public string? ExistingImageFileName { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var frame = await catalogService.GetAsync(id, cancellationToken);
        if (frame is null)
        {
            return NotFound();
        }

        FrameId = id;
        ExistingImageFileName = frame.ImageFileName;
        Input = FrameInputModel.FromCatalogItem(frame);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        FrameId = id;
        var existingFrame = await catalogService.GetAsync(id, cancellationToken);
        if (existingFrame is null)
        {
            return NotFound();
        }

        ExistingImageFileName = existingFrame.ImageFileName;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? extension = null;
        if (Input.Image is not null)
        {
            try
            {
                extension = await FrameImageValidator.ValidateAsync(Input.Image, cancellationToken);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError("Input.Image", exception.Message);
                return Page();
            }
        }

        try
        {
            await catalogService.UpdateAsync(id, Input.ToUpdateRequest(), cancellationToken);

            if (Input.Image is not null && extension is not null)
            {
                await using var content = Input.Image.OpenReadStream();
                var newFileName = await imageStorage.SaveAsync(content, extension, cancellationToken);
                try
                {
                    await catalogService.SetImageAsync(id, newFileName, cancellationToken);
                }
                catch
                {
                    await imageStorage.DeleteAsync(newFileName, cancellationToken);
                    throw;
                }

                if (existingFrame.ImageFileName is not null)
                {
                    await imageStorage.DeleteAsync(existingFrame.ImageFileName, cancellationToken);
                }
            }
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
