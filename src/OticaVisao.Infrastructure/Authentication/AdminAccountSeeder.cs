using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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

        await EnsureRoleAsync(AdminAuthorization.Role, "Não foi possível criar a função administrativa.");
        await EnsureRoleAsync(AdminAuthorization.GeneralRole, "Não foi possível criar a função de administrador geral.");

        for (var index = 0; index < accounts.Count; index++)
        {
            var account = accounts[index];
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

            if (index > 0 && !await HasPanelPermissionAsync(user))
            {
                EnsureSucceeded(await userManager.AddClaimAsync(user,
                        new Claim(AdminAuthorization.PermissionClaim, AdminAuthorization.AllPanelsPermission)),
                    $"Não foi possível liberar os painéis para '{normalizedEmail}'.");
            }
        }

        var generalAdministrators = await userManager.GetUsersInRoleAsync(AdminAuthorization.GeneralRole);
        if (generalAdministrators.Count == 0)
        {
            var primaryEmail = accounts[0].Email.Trim().ToLowerInvariant();
            var primaryAdministrator = await userManager.FindByEmailAsync(primaryEmail)
                ?? throw new InvalidOperationException("A conta do administrador geral não foi encontrada.");
            EnsureSucceeded(await userManager.AddToRoleAsync(primaryAdministrator, AdminAuthorization.GeneralRole),
                $"Não foi possível autorizar o administrador geral '{primaryEmail}'.");
        }
        foreach (var administrator in await userManager.GetUsersInRoleAsync(AdminAuthorization.Role))
        {
            if (await userManager.IsInRoleAsync(administrator, AdminAuthorization.GeneralRole)
                || await HasPanelPermissionAsync(administrator)) continue;
            EnsureSucceeded(await userManager.AddClaimAsync(administrator,
                    new Claim(AdminAuthorization.PermissionClaim, AdminAuthorization.AllPanelsPermission)),
                $"Não foi possível preservar o acesso existente de '{administrator.Email}'.");
        }
    }

    private async Task<bool> HasPanelPermissionAsync(ApplicationUser user) =>
        (await userManager.GetClaimsAsync(user)).Any(claim =>
            claim.Type == AdminAuthorization.PermissionClaim);

    private async Task EnsureRoleAsync(string role, string errorMessage)
    {
        if (await roleManager.RoleExistsAsync(role))
        {
            return;
        }

        EnsureSucceeded(
            await roleManager.CreateAsync(new IdentityRole<Guid>(role)),
            errorMessage);
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
