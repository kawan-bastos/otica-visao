using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Application.Engagement;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages;

public class IndexModel(FrameCatalogService catalogService, ICustomerEngagementService engagementService, UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<FrameCatalogItem> FeaturedFrames { get; private set; } = [];
    public IReadOnlySet<Guid> FavoriteIds { get; private set; } = new HashSet<Guid>();
    public IReadOnlySet<Guid> ReservedIds { get; private set; } = new HashSet<Guid>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        FeaturedFrames = await catalogService.ListPublicAsync(
            new PublicFrameFilter(),
            maximumItems: 4,
            cancellationToken);
        if (!User.IsInRole(AdminAuthorization.Role) && userManager.GetUserId(User) is { } value && Guid.TryParse(value, out var userId))
        {
            FavoriteIds = await engagementService.FavoriteFrameIdsAsync(userId, cancellationToken);
            ReservedIds = await engagementService.ActiveReservationFrameIdsAsync(userId, cancellationToken);
        }
    }

    public async Task<IActionResult> OnPostFavoriteAsync(Guid frameId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        await engagementService.ToggleFavoriteAsync(userId, frameId, cancellationToken);
        TempData["HomeMessage"] = "Seus favoritos foram atualizados.";
        return RedirectToPage("/Index", null, "armacoes");
    }

    public async Task<IActionResult> OnPostReserveAsync(Guid frameId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        try { await engagementService.ReserveAsync(userId, frameId, null, cancellationToken); TempData["HomeMessage"] = "Armação reservada por 24 horas."; }
        catch (InvalidOperationException ex) { TempData["HomeError"] = ex.Message; }
        return RedirectToPage("/Index", null, "armacoes");
    }
}
