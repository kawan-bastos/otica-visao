using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OticaVisao.Infrastructure.Authentication;

public sealed class AdminAccountSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<AdminAccountOptions> options)
{
    public async Task SeedAsync()
    {
        var accounts = options.Value.Accounts;
        if (accounts.Count == 0)
        {
            return;
        }

        await EnsureRoleAsync();

        foreach (var account in accounts)
        {
            Validate(account);

            var normalizedEmail = account.Email.Trim().ToLowerInvariant();
            var user = await userManager.FindByEmailAsync(normalizedEmail);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    EmailConfirmed = true,
                    DisplayName = account.DisplayName.Trim()
                };

                EnsureSucceeded(
                    await userManager.CreateAsync(user, account.Password),
                    $"Não foi possível criar o administrador '{normalizedEmail}'.");
            }

            if (!await userManager.IsInRoleAsync(user, AdminAuthorization.Role))
            {
                EnsureSucceeded(
                    await userManager.AddToRoleAsync(user, AdminAuthorization.Role),
                    $"Não foi possível autorizar o administrador '{normalizedEmail}'.");
            }
        }
    }

    private async Task EnsureRoleAsync()
    {
        if (await roleManager.RoleExistsAsync(AdminAuthorization.Role))
        {
            return;
        }

        EnsureSucceeded(
            await roleManager.CreateAsync(new IdentityRole<Guid>(AdminAuthorization.Role)),
            "Não foi possível criar a função administrativa.");
    }

    private static void Validate(AdminAccountConfiguration account)
    {
        if (string.IsNullOrWhiteSpace(account.DisplayName)
            || string.IsNullOrWhiteSpace(account.Email)
            || string.IsNullOrWhiteSpace(account.Password))
        {
            throw new InvalidOperationException(
                "Cada administrador inicial deve possuir nome, e-mail e senha configurados.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {errors}");
    }
}
