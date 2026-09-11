using Microsoft.Extensions.Configuration;
using OticaVisao.Web.Configuration;

namespace OticaVisao.Tests.Web;

public sealed class ProductionConfigurationValidatorTests
{
    [Fact]
    public void AcceptsCompleteProductionConfiguration()
    {
        var configuration = Configuration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=database.internal;Database=otica;Username=app;Password=secret",
            ["Database:ApplyMigrations"] = "true",
            ["FrameImageStorage:Path"] = Path.Combine(Path.GetTempPath(), "frame-images"),
            ["LaboratoryDocumentStorage:Path"] = Path.Combine(Path.GetTempPath(), "laboratory-documents"),
            ["DataProtection:KeysPath"] = Path.Combine(Path.GetTempPath(), "data-protection-keys"),
            ["Email:Host"] = "smtp.example.com",
            ["Email:Port"] = "587",
            ["Email:UserName"] = "mailer@example.com",
            ["Email:Password"] = "smtp-app-password",
            ["Email:FromAddress"] = "contato@example.com",
            ["Email:UseSsl"] = "true",
            ["AllowedHosts"] = "otica.example.com",
            ["AdminAccounts:Accounts:0:DisplayName"] = "Carlos",
            ["AdminAccounts:Accounts:0:Email"] = "carlos@example.com",
            ["AdminAccounts:Accounts:0:Password"] = "Senha-Forte-123!"
        });

        ProductionConfigurationValidator.Validate(configuration);
    }

    [Fact]
    public void RejectsUnsafeProductionConfiguration()
    {
        var configuration = Configuration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=otica;Username=postgres",
            ["Database:ApplyMigrations"] = "false",
            ["FrameImageStorage:Path"] = "frame-images",
            ["AllowedHosts"] = "*"
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration));

        Assert.Contains("Configuração de produção insegura", exception.Message);
        Assert.Contains("conexão de produção", exception.Message);
        Assert.Contains("AllowedHosts", exception.Message);
        Assert.Contains("Email:UserName", exception.Message);
    }

    [Fact]
    public void RejectsIncompleteOrInsecureEmailConfiguration()
    {
        var configuration = Configuration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=database.internal;Database=otica;Username=app;Password=secret",
            ["Database:ApplyMigrations"] = "true",
            ["FrameImageStorage:Path"] = Path.Combine(Path.GetTempPath(), "frame-images"),
            ["LaboratoryDocumentStorage:Path"] = Path.Combine(Path.GetTempPath(), "laboratory-documents"),
            ["DataProtection:KeysPath"] = Path.Combine(Path.GetTempPath(), "data-protection-keys"),
            ["Email:Host"] = "smtp.example.com",
            ["Email:Port"] = "0",
            ["Email:FromAddress"] = "contato@example.com",
            ["Email:UseSsl"] = "false",
            ["AllowedHosts"] = "otica.example.com",
            ["AdminAccounts:Accounts:0:DisplayName"] = "Carlos",
            ["AdminAccounts:Accounts:0:Email"] = "carlos@example.com",
            ["AdminAccounts:Accounts:0:Password"] = "Senha-Forte-123!"
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProductionConfigurationValidator.Validate(configuration));

        Assert.Contains("Email:UserName", exception.Message);
        Assert.Contains("Email:Port", exception.Message);
        Assert.Contains("Email:UseSsl", exception.Message);
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
