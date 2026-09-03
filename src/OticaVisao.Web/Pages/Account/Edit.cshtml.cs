using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[Authorize]
public sealed class EditModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public CustomerProfileInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return Forbid();

        var customer = await context.Customers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.AccountUserId == user.Id);
        Input = FromRecords(user, customer);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null || await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return Forbid();
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim().ToLowerInvariant();
        var cpf = BrazilianCpf.Normalize(Input.Cpf);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var linkedCustomer = await context.Customers.SingleOrDefaultAsync(item => item.AccountUserId == user.Id);
        var cpfCustomer = await context.Customers.SingleOrDefaultAsync(item => item.Cpf == cpf);
        if (linkedCustomer is not null && cpfCustomer is not null && linkedCustomer.Id != cpfCustomer.Id)
        {
            ModelState.AddModelError("Input.Cpf", "Este CPF já pertence a outra ficha de cliente.");
            return Page();
        }

        var customer = linkedCustomer ?? cpfCustomer;
        if (linkedCustomer is null && customer is not null && !CanAssociate(customer))
        {
            ModelState.AddModelError(string.Empty,
                "Já existe uma ficha com este CPF. Confira telefone e data de nascimento ou fale com a loja.");
            return Page();
        }

        var address = new CustomerAddress(
            Input.PostalCode, Input.Street, Input.Number, Input.Complement,
            Input.Neighborhood, Input.City, Input.State);
        if (customer is null)
        {
            customer = new Customer(
                Input.DisplayName, Input.PhoneNumber, cpf, Input.BirthDate!.Value, address, email);
            context.Customers.Add(customer);
        }
        else
        {
            customer.Update(
                Input.DisplayName, Input.PhoneNumber, cpf, Input.BirthDate!.Value,
                address, email, customer.Notes);
        }
        customer.LinkToAccount(user.Id);

        user.DisplayName = Input.DisplayName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = Input.PhoneNumber.Trim();

        try
        {
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
                return Page();
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar os dados agora. Tente novamente.");
            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["AccountSuccessMessage"] = "Seus dados foram atualizados.";
        return RedirectToPage("Index");
    }

    private bool CanAssociate(Customer customer) =>
        !customer.AccountUserId.HasValue
        && customer.BirthDate == Input.BirthDate
        && OnlyDigits(customer.Phone) == OnlyDigits(Input.PhoneNumber);

    private static CustomerProfileInputModel FromRecords(ApplicationUser user, Customer? customer) => new()
    {
        DisplayName = customer?.Name ?? user.DisplayName,
        Email = customer?.Email ?? user.Email ?? string.Empty,
        PhoneNumber = customer?.Phone ?? user.PhoneNumber ?? string.Empty,
        Cpf = FormatCpf(customer?.Cpf),
        BirthDate = customer?.BirthDate,
        PostalCode = customer?.Address?.PostalCode ?? string.Empty,
        Street = customer?.Address?.Street ?? string.Empty,
        Number = customer?.Address?.Number ?? string.Empty,
        Complement = customer?.Address?.Complement,
        Neighborhood = customer?.Address?.Neighborhood ?? string.Empty,
        City = customer?.Address?.City ?? string.Empty,
        State = customer?.Address?.State ?? "RJ"
    };

    private static string OnlyDigits(string value) => new(value.Where(char.IsDigit).ToArray());
    private static string FormatCpf(string? value) => value?.Length == 11
        ? $"{value[..3]}.{value[3..6]}.{value[6..9]}-{value[9..]}"
        : string.Empty;

    private static string TranslateIdentityError(string code) => code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Já existe uma conta com este e-mail.",
        "InvalidEmail" => "Informe um e-mail válido.",
        _ => "Não foi possível atualizar a conta. Confira os dados e tente novamente."
    };
}
