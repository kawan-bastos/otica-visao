using Microsoft.AspNetCore.Identity;

namespace OticaVisao.Infrastructure.Authentication;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
