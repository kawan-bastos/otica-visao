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

[AllowAnonymous]
public sealed class RegisterModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context) : PageModel
{
    [BindProperty] public CustomerRegisterInputModel Input { get; set; } = new();

    public IActionResult OnGet() => User.Identity?.IsAuthenticated == true ? RedirectToPage("Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim().ToLowerInvariant();
        var cpf = BrazilianCpf.Normalize(Input.Cpf);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var customer = await context.Customers.SingleOrDefaultAsync(item => item.Cpf == cpf);
        if (customer is not null && !CanAssociate(customer))
        {
            ModelState.AddModelError(string.Empty,
                "Já existe uma ficha com este CPF. Confira telefone e data de nascimento ou fale com a loja.");
            return Page();
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            DisplayName = Input.DisplayName.Trim(),
            PhoneNumber = Input.PhoneNumber.Trim()
        };
        var result = await userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            var address = new CustomerAddress(
                Input.PostalCode, Input.Street, Input.Number, Input.Complement,
                Input.Neighborhood, Input.City, Input.State);

            if (customer is null)
            {
                customer = new Customer(
                    Input.DisplayName, Input.PhoneNumber, cpf, Input.BirthDate!.Value,
                    address, email);
                context.Customers.Add(customer);
            }
            else
            {
                customer.Update(
                    Input.DisplayName, Input.PhoneNumber, cpf, Input.BirthDate!.Value,
                    address, email, customer.Notes);
            }

            customer.LinkToAccount(user.Id);
            try
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty,
                    "Não foi possível associar o cadastro. Confira os dados ou fale com a loja.");
                return Page();
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToPage("Index");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, TranslateIdentityError(error.Code));
        return Page();
    }

    private bool CanAssociate(Customer customer) =>
        !customer.AccountUserId.HasValue
        && customer.BirthDate == Input.BirthDate
        && OnlyDigits(customer.Phone) == OnlyDigits(Input.PhoneNumber);

    private static string OnlyDigits(string value) => new(value.Where(char.IsDigit).ToArray());

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
