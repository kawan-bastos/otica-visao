using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[AllowAnonymous]
public sealed class RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : PageModel
{
    [BindProperty] public CustomerRegisterInputModel Input { get; set; } = new();

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim().ToLowerInvariant();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = Input.DisplayName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim()
        };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("Index");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
        return Page();
    }

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Já existe uma conta com este e-mail.",
        "PasswordTooShort" => "A senha deve ter pelo menos 8 caracteres.",
        "PasswordRequiresDigit" => "A senha deve possuir pelo menos um número.",
        "PasswordRequiresLower" => "A senha deve possuir pelo menos uma letra minúscula.",
        "PasswordRequiresUpper" => "A senha deve possuir pelo menos uma letra maiúscula.",
        "PasswordRequiresNonAlphanumeric" => "A senha deve possuir pelo menos um símbolo.",
        _ => "Não foi possível criar a conta. Confira os dados e tente novamente."
    };
}
