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
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
