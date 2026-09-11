using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Engagement;
using OticaVisao.Domain.Engagement;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class ReservationsModel(ICustomerEngagementService engagementService, UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<ReservationItem> Reservations { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (User.IsInRole(AdminAuthorization.Role)) return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));
        var reservations = await engagementService.ListReservationsAsync(UserId(), ct);
        Reservations = reservations
            .Where(item => item.Status is not FrameReservationStatus.Cancelled and not FrameReservationStatus.Expired)
            .ToArray();
        return Page();
    }
    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken ct)
    {
        if (User.IsInRole(AdminAuthorization.Role)) return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));
        try { await engagementService.CancelAsync(UserId(), id, ct); TempData["AccountSuccessMessage"] = "Reserva cancelada e armação liberada."; }
        catch (InvalidOperationException ex) { TempData["AccountErrorMessage"] = ex.Message; }
        return RedirectToPage();
    }
    private Guid UserId() => Guid.Parse(userManager.GetUserId(User) ?? throw new InvalidOperationException("Usuário não encontrado."));
}
