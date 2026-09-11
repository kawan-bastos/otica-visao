using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;
using Microsoft.AspNetCore.Identity;
using OticaVisao.Application.Engagement;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Frames;

public sealed class DetailsModel(FrameCatalogService catalogService, ICustomerEngagementService engagementService, UserManager<ApplicationUser> userManager) : PageModel
{
    public FrameCatalogItem Frame { get; private set; } = null!;

    public string WhatsAppUrl { get; private set; } = string.Empty;
    public bool IsFavorite { get; private set; }
    public bool IsReserved { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var frame = await catalogService.GetPublicAsync(id, cancellationToken);
        if (frame is null)
        {
            return NotFound();
        }

        Frame = frame;
        var message = $"Olá! Vi a armação {frame.Brand} {frame.Model}, código {frame.Code}, no site e gostaria de confirmar a disponibilidade.";
        WhatsAppUrl = $"https://wa.me/5521965912440?text={Uri.EscapeDataString(message)}";
        if (!User.IsInRole(AdminAuthorization.Role) && Guid.TryParse(userManager.GetUserId(User), out var userId))
        {
            IsFavorite = (await engagementService.FavoriteFrameIdsAsync(userId, cancellationToken)).Contains(id);
            IsReserved = (await engagementService.ActiveReservationFrameIdsAsync(userId, cancellationToken)).Contains(id);
        }
        return Page();
    }

    public async Task<IActionResult> OnPostFavoriteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        await engagementService.ToggleFavoriteAsync(userId, id, cancellationToken);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostReserveAsync(Guid id, string? visitPeriod, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(userManager.GetUserId(User), out var userId)) return Challenge();
        try { await engagementService.ReserveAsync(userId, id, visitPeriod, cancellationToken); TempData["CatalogMessage"] = "Armação reservada por 24 horas."; }
        catch (InvalidOperationException ex) { TempData["CatalogError"] = ex.Message; }
        return RedirectToPage(new { id });
    }

    public static string TypeName(FrameType type) => type == FrameType.Sunglasses
        ? "Óculos de sol"
        : "Óculos de grau";
}
