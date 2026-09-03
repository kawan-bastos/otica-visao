using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class DeleteModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty]
    public DeleteAccountInputModel Input { get; set; } = new();

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

        if (!await userManager.CheckPasswordAsync(user, Input.Password))
        {
            ModelState.AddModelError(string.Empty, "A senha informada está incorreta.");
            return Page();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível excluir sua conta agora. Tente novamente.");
            return Page();
        }

        await signInManager.SignOutAsync();
        return RedirectToPage("Deleted");
    }
}
