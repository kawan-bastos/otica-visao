using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Images;

namespace OticaVisao.Web.Configuration;

public static class ProductionConfigurationValidator
{
    public static void Validate(IConfiguration configuration)
    {
        var errors = new List<string>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString)) errors.Add("Configure ConnectionStrings:DefaultConnection.");
        else if (connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase)
                 || connectionString.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase))
            errors.Add("A conexão de produção não pode apontar para o computador local.");

        if (!configuration.GetValue<bool>("Database:ApplyMigrations"))
            errors.Add("Ative Database:ApplyMigrations em produção.");

        var imagePath = configuration[$"{FrameImageStorageOptions.SectionName}:Path"];
        if (string.IsNullOrWhiteSpace(imagePath) || !Path.IsPathRooted(imagePath))
            errors.Add("Configure FrameImageStorage:Path com um caminho absoluto e persistente.");

        if (configuration["AllowedHosts"] is not { Length: > 0 } allowedHosts || allowedHosts == "*")
            errors.Add("Restrinja AllowedHosts ao domínio usado em produção.");

        var accounts = configuration.GetSection(AdminAccountOptions.SectionName)
            .Get<AdminAccountOptions>()?.Accounts ?? [];
        if (accounts.Count == 0) errors.Add("Configure ao menos uma conta administrativa.");
        foreach (var account in accounts)
        {
            if (string.IsNullOrWhiteSpace(account.DisplayName) || string.IsNullOrWhiteSpace(account.Email))
                errors.Add("Toda conta administrativa precisa de nome e e-mail.");
            if (string.IsNullOrWhiteSpace(account.Password) || account.Password.Length < 12)
                errors.Add("Toda senha administrativa de produção deve possuir ao menos 12 caracteres.");
        }

        if (errors.Count > 0)
            throw new InvalidOperationException("Configuração de produção insegura: " + string.Join(" ", errors));
    }
}
