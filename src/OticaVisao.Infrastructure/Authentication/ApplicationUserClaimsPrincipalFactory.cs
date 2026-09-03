using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OticaVisao.Infrastructure.Authentication;

internal sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.DisplayName))
        {
            identity.AddClaim(new Claim(AdminAuthorization.DisplayNameClaim, user.DisplayName.Trim()));
        }
        return identity;
    }
}
