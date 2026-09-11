using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Engagement;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Account;
[Authorize]
public sealed class FavoritesModel(ICustomerEngagementService service, UserManager<ApplicationUser> users) : PageModel
{
    public IReadOnlyList<EngagementFrameItem> Favorites { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (User.IsInRole(AdminAuthorization.Role)) return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));
        Favorites = await service.ListFavoritesAsync(UserId(), ct);
        return Page();
    }
    public async Task<IActionResult> OnPostRemoveAsync(Guid frameId, CancellationToken ct)
    {
        if (User.IsInRole(AdminAuthorization.Role)) return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));
        await service.ToggleFavoriteAsync(UserId(), frameId, ct);
        return RedirectToPage();
    }
    private Guid UserId() => Guid.Parse(users.GetUserId(User) ?? throw new InvalidOperationException("Usuário não encontrado."));
}
