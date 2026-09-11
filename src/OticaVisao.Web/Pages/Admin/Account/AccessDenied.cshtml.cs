using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Admin.Account;

[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    public string PanelLandingPage { get; private set; } = "/Account/Index";

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AdminAuthorization.Role))
            {
                PanelLandingPage = AdminAuthorization.GetPanelLandingPage(User);
                return Page();
            }

            TempData["AccountNoticeMessage"] = "Sua conta não possui permissão para acessar o painel administrativo.";
            return RedirectToPage("/Account/Index");
        }

        return Page();
    }
}
