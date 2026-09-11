using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Tests.Authentication;

public sealed class AdminAccountSeederTests
{
    [Fact]
    public async Task SeederCreatesTwoAdministratorsWithHashedPasswordsAndIsIdempotent()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"identity-{Guid.NewGuid()}"));
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.Configure<AdminAccountOptions>(options =>
        {
            options.Accounts =
            [
                new AdminAccountConfiguration
                {
                    DisplayName = "Proprietário",
                    Email = "proprietario@example.com",
                    Password = "Senha-Forte-123!"
                },
                new AdminAccountConfiguration
                {
                    DisplayName = "Administrador",
                    Email = "administrador@example.com",
                    Password = "Outra-Senha-456!"
                }
            ];
        });
        services.AddScoped<AdminAccountSeeder>();

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminAccountSeeder>();

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var users = await userManager.Users.OrderBy(user => user.Email).ToListAsync();

        Assert.Equal(2, users.Count);
        Assert.All(users, user => Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash)));
        foreach (var user in users)
        {
            Assert.True(await userManager.IsInRoleAsync(user, AdminAuthorization.Role));
        }

        var generalAdministrators = new List<ApplicationUser>();
        foreach (var user in users)
        {
            if (await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole)) generalAdministrators.Add(user);
        }
        Assert.Equal("proprietario@example.com", Assert.Single(generalAdministrators).Email);

        var ordinaryAdministrator = users.Single(user => user.Email == "administrador@example.com");
        var ordinaryClaims = await userManager.GetClaimsAsync(ordinaryAdministrator);
        Assert.Contains(ordinaryClaims, claim => claim.Type == AdminAuthorization.PermissionClaim
            && claim.Value == AdminAuthorization.AllPanelsPermission);

        Assert.True(await userManager.CheckPasswordAsync(users[1], "Senha-Forte-123!"));
    }
}
