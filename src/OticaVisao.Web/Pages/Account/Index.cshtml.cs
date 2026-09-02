using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("Usuário autenticado não encontrado.");
        DisplayName = user.DisplayName;
        Email = user.Email ?? string.Empty;
        PhoneNumber = user.PhoneNumber;
    }
}
