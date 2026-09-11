using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Account;

[AllowAnonymous]
public sealed class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AdminAuthorization.Role))
                return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));

            TempData["AccountNoticeMessage"] = "Sua conta não possui permissão para acessar o painel administrativo.";
            return RedirectToPage("/Account/Index");
        }

        ReturnUrl = LocalReturnUrl(returnUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = LocalReturnUrl(returnUrl);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email.Trim().ToLowerInvariant(),
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await userManager.FindByEmailAsync(Input.Email.Trim());
            if (user is not null && await userManager.IsInRoleAsync(user, AdminAuthorization.Role))
            {
                return LocalRedirect(ReturnUrl ?? Url.Page(await PanelLandingAsync(user))!);
            }

            if (user is not null)
            {
                TempData["AccountNoticeMessage"] = "Login realizado, mas sua conta não possui permissão para acessar o painel administrativo. Você foi direcionado para sua conta.";
                return RedirectToPage("/Account/Index");
            }

            await signInManager.SignOutAsync();
        }

        ModelState.AddModelError(
            string.Empty,
            result.IsLockedOut
                ? "Acesso temporariamente bloqueado após várias tentativas. Tente novamente mais tarde."
                : "E-mail ou senha inválidos.");

        return Page();
    }

    private string? LocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;

    private async Task<string> PanelLandingAsync(ApplicationUser user)
    {
        if (await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole)) return "/Admin/Frames/Index";
        var permissions = (await userManager.GetClaimsAsync(user))
            .Where(claim => claim.Type == AdminAuthorization.PermissionClaim).Select(claim => claim.Value).ToHashSet();
        if (permissions.Contains(AdminAuthorization.AllPanelsPermission) || permissions.Contains(AdminAuthorization.FramesPermission)) return "/Admin/Frames/Index";
        return AdminAuthorization.Permissions.FirstOrDefault(permission => permissions.Contains(permission.Value))?.Value switch
        {
            AdminAuthorization.CustomersPermission => "/Admin/Customers/Index",
            AdminAuthorization.SalesPermission => "/Admin/Sales/Index",
            AdminAuthorization.ReservationsPermission => "/Admin/Reservations/Index",
            AdminAuthorization.LaboratoryOrdersPermission => "/Admin/LaboratoryOrders/Index",
            AdminAuthorization.ReportsPermission => "/Admin/Reports/Index",
            AdminAuthorization.AuditPermission => "/Admin/Audit/Index",
            _ => "/Admin/Account/AccessDenied"
        };
    }
}
