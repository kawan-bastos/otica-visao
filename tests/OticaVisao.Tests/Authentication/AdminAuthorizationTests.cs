using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OticaVisao.Tests.Authentication;

public sealed class AdminAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public AdminAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var accountInitializer = services
                    .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                    .Single(descriptor =>
                        descriptor.ImplementationType?.Name == "AdminAccountInitializationService");
                services.Remove(accountInitializer);
                services.AddLogging(logging => logging.ClearProviders());
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
            });
        });
    }

    [Fact]
    public async Task AnonymousVisitorIsRedirectedFromAdminToLogin()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/Admin/Frames");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task LoginPageIsAvailableWithoutAuthentication()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/Admin/Account/Login");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
    }
}
