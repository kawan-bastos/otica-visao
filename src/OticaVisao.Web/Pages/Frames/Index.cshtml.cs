using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;
using Microsoft.AspNetCore.Identity;
using OticaVisao.Application.Engagement;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Frames;

public sealed class IndexModel(FrameCatalogService catalogService, ICustomerEngagementService engagementService, UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public FrameType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public FrameShape? Shape { get; set; }

    [BindProperty(SupportsGet = true)]
    public TargetAudience? TargetAudience { get; set; }

    public IReadOnlyList<FrameCatalogItem> Frames { get; private set; } = [];
    public IReadOnlySet<Guid> FavoriteIds { get; private set; } = new HashSet<Guid>();
    public IReadOnlySet<Guid> ReservedIds { get; private set; } = new HashSet<Guid>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Frames = await catalogService.ListPublicAsync(
            new PublicFrameFilter(Search, Type, Shape, TargetAudience),
            cancellationToken: cancellationToken);
        if (!User.IsInRole(AdminAuthorization.Role)
            && userManager.GetUserId(User) is { } value
            && Guid.TryParse(value, out var userId))
        {
            FavoriteIds = await engagementService.FavoriteFrameIdsAsync(userId, cancellationToken);
            ReservedIds = await engagementService.ActiveReservationFrameIdsAsync(userId, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostFavoriteAsync(Guid frameId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        await engagementService.ToggleFavoriteAsync(userId, frameId, cancellationToken);
        var isFavorite = (await engagementService.FavoriteFrameIdsAsync(userId, cancellationToken)).Contains(frameId);
        if (Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult(new { success = true, isFavorite });
        }

        TempData["CatalogMessage"] = "Seus favoritos foram atualizados.";
        return RedirectToPage(new { Search, Type, Shape, TargetAudience });
    }

    public async Task<IActionResult> OnPostReserveAsync(Guid frameId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        try { await engagementService.ReserveAsync(userId, frameId, null, cancellationToken); TempData["CatalogMessage"] = "Armação reservada por 24 horas."; }
        catch (InvalidOperationException ex) { TempData["CatalogError"] = ex.Message; }
        return RedirectToPage(new { Search, Type, Shape, TargetAudience });
    }

    public static string TypeName(FrameType type) => type == FrameType.Sunglasses
        ? "Óculos de sol"
        : "Óculos de grau";
}
