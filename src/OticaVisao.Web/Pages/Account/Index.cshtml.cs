using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context) : PageModel
{
    public string DisplayName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string Cpf { get; private set; } = string.Empty;
    public DateOnly? BirthDate { get; private set; }
    public string Address { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("Usuário autenticado não encontrado.");
        DisplayName = user.DisplayName;
        Email = user.Email ?? string.Empty;
        PhoneNumber = user.PhoneNumber;
        var customer = await context.Customers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.AccountUserId == user.Id);
        if (customer is null) return;

        Cpf = FormatCpf(customer.Cpf);
        BirthDate = customer.BirthDate;
        if (customer.Address is not null)
        {
            var complement = string.IsNullOrWhiteSpace(customer.Address.Complement) ? string.Empty : $", {customer.Address.Complement}";
            Address = $"{customer.Address.Street}, {customer.Address.Number}{complement} — {customer.Address.Neighborhood}, {customer.Address.City}/{customer.Address.State} — CEP {customer.Address.PostalCode}";
        }
    }

    private static string FormatCpf(string? value) => value?.Length == 11
        ? $"{value[..3]}.{value[3..6]}.{value[6..9]}-{value[9..]}"
        : string.Empty;
}
