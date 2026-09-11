using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class ChangePasswordModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    PasswordHistoryService passwordHistory) : PageModel
{
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user is null || await userManager.IsInRoleAsync(user, AdminAuthorization.Role)
            ? Forbid()
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return Forbid();
        if (!ModelState.IsValid) return Page();

        if (await passwordHistory.WasPreviouslyUsedAsync(user, Input.NewPassword, HttpContext.RequestAborted))
        {
            ModelState.AddModelError("Input.NewPassword", "Escolha uma senha que não tenha sido usada anteriormente.");
            return Page();
        }

        var previousPasswordHash = user.PasswordHash;
        var result = await userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Code == "PasswordMismatch"
                    ? "A senha atual está incorreta."
                    : "A nova senha não atende aos requisitos de segurança.");
            return Page();
        }

        await passwordHistory.RecordPreviousPasswordAsync(user, previousPasswordHash, HttpContext.RequestAborted);
        await signInManager.RefreshSignInAsync(user);
        TempData["AccountSuccessMessage"] = "Sua senha foi alterada com segurança.";
        return RedirectToPage("Index");
    }
}
