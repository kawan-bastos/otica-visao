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

    [Fact]
    public async Task AnonymousVisitorIsRedirectedFromCustomerAdministrationToLogin()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/Admin/Customers");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task AnonymousVisitorIsRedirectedFromSalesToLogin()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/Admin/Sales");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Admin/Account/Login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task AccountPagesPreventBrowserCaching()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.GetAsync("/Account/Login");

        Assert.Contains("no-store", response.Headers.CacheControl?.ToString());
        Assert.True(response.Headers.TryGetValues("X-Content-Type-Options", out var values));
        Assert.Contains("nosniff", values);
    }

    [Fact]
    public async Task AuthenticationSubmissionsAreRateLimited()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        HttpResponseMessage? response = null;
        for (var attempt = 0; attempt < 11; attempt++)
        {
            response = await client.PostAsync("/Admin/Account/Login", new FormUrlEncodedContent([]));
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, response!.StatusCode);
        Assert.Contains("Muitas tentativas", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    public async Task CustomerAccountPagesAreAvailableWithoutAuthentication(string path)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
    }

    [Theory]
    [InlineData("/Account")]
    [InlineData("/Account/Edit")]
    [InlineData("/Account/Delete")]
    public async Task AnonymousVisitorIsRedirectedFromProtectedCustomerPagesToCustomerLogin(string path)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location?.AbsolutePath);
    }
}
