using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;

namespace OticaVisao.Web.Pages.Admin.Frames;

public sealed class IndexModel(FrameCatalogService catalogService) : PageModel
{
    public IReadOnlyList<FrameCatalogItem> Frames { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    public int TotalFrames { get; private set; }

    public int AvailableFrames { get; private set; }

    public int LowStockFrames { get; private set; }

    public int OutOfStockFrames { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var frames = await catalogService.ListAsync(cancellationToken);
        TotalFrames = frames.Count;
        AvailableFrames = frames.Count(frame => frame.IsAvailable);
        LowStockFrames = frames.Count(frame => frame.IsAvailable && frame.StockQuantity <= 2);
        OutOfStockFrames = frames.Count(frame => frame.StockQuantity == 0);

        IEnumerable<FrameCatalogItem> filteredFrames = frames;
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            filteredFrames = filteredFrames.Where(frame =>
                frame.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                || frame.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)
                || frame.Model.Contains(search, StringComparison.OrdinalIgnoreCase)
                || frame.Color.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        filteredFrames = Status switch
        {
            "available" => filteredFrames.Where(frame => frame.IsAvailable),
            "low" => filteredFrames.Where(frame => frame.IsAvailable && frame.StockQuantity <= 2),
            "out" => filteredFrames.Where(frame => frame.StockQuantity == 0),
            "hidden" => filteredFrames.Where(frame => !frame.IsActive || !frame.IsPublished),
            _ => filteredFrames
        };

        Frames = filteredFrames.ToArray();
    }

    public async Task<IActionResult> OnPostIncreaseStockAsync(
        Guid id,
        string? search,
        string? status,
        CancellationToken cancellationToken)
    {
        await catalogService.AddToStockAsync(id, 1, cancellationToken);
        TempData["SuccessMessage"] = "Uma unidade foi adicionada ao estoque.";
        return RedirectToPage(new { search, status });
    }

    public async Task<IActionResult> OnPostDecreaseStockAsync(
        Guid id,
        string? search,
        string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            await catalogService.RemoveFromStockAsync(id, 1, cancellationToken);
            TempData["SuccessMessage"] = "Uma unidade foi retirada do estoque.";
        }
        catch (InvalidOperationException)
        {
            TempData["ErrorMessage"] = "O estoque desta armação já está zerado.";
        }

        return RedirectToPage(new { search, status });
    }
}
