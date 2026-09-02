using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty] public CustomerLoginInputModel Input { get; set; } = new();
    public string? ReturnUrl { get; private set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("Index");
        ReturnUrl = LocalReturnUrl(returnUrl);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = LocalReturnUrl(returnUrl);
        if (!ModelState.IsValid) return Page();

        var result = await signInManager.PasswordSignInAsync(
            Input.Email.Trim().ToLowerInvariant(), Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirect(ReturnUrl ?? Url.Page("Index")!);

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Acesso temporariamente bloqueado. Tente novamente mais tarde."
            : "E-mail ou senha inválidos.");
        return Page();
    }

    private string? LocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;
}
