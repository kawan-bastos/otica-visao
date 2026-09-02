namespace OticaVisao.Infrastructure.Authentication;

public sealed class AdminAccountOptions
{
    public const string SectionName = "AdminAccounts";

    public List<AdminAccountConfiguration> Accounts { get; set; } = [];
}

public sealed class AdminAccountConfiguration
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
