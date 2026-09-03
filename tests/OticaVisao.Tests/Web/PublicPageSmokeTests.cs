using System.Net;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Tests.Web;

public sealed class PublicPageSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;
    private static readonly Guid FrameId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public PublicPageSmokeTests(WebApplicationFactory<Program> factory)
    {
        var configuredFactory = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            var initializer = services.Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                .Single(descriptor => descriptor.ImplementationType?.Name == "AdminAccountInitializationService");
            services.Remove(initializer);
            services.Remove(services.Single(descriptor => descriptor.ServiceType == typeof(IFrameRepository)));
            services.AddSingleton<IFrameRepository>(new FakeFrameRepository(CreateFrame()));
            services.AddLogging(logging => logging.ClearProviders());
            services.AddDataProtection().UseEphemeralDataProtectionProvider();
        }));
        client = configuredFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Frames")]
    [InlineData("/Frames?type=Prescription&shape=Round")]
    [InlineData("/sobre-nos")]
    [InlineData("/contato")]
    [InlineData("/Privacy")]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/Admin/Account/Login")]
    public async Task EssentialPublicPagesRespondSuccessfully(string route)
    {
        var response = await client.GetAsync(route);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Ótica Visão", body);
    }

    [Fact]
    public async Task PublishedFrameDetailsAreAvailable()
    {
        var response = await client.GetAsync($"/Frames/Details/{FrameId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("VAL-001", body);
    }

    [Fact]
    public async Task UnknownFrameReturnsNotFound()
    {
        var response = await client.GetAsync($"/Frames/Details/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static Frame CreateFrame()
    {
        var frame = new Frame("VAL-001", "Linha Visão", "Modelo de validação", "Preto", 219m, 1,
            FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);
        typeof(Frame).GetProperty(nameof(Frame.Id))!.SetValue(frame, FrameId);
        frame.Publish();
        return frame;
    }

    private sealed class FakeFrameRepository(Frame frame) : IFrameRepository
    {
        private readonly Frame[] frames = [frame];
        public Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Frame>>(frames);
        public Task<IReadOnlyList<Frame>> ListPublicAsync(PublicFrameFilter filter, int? maximumItems = null, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Frame>>(frames.Take(maximumItems ?? frames.Length).ToArray());
        public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(frames.SingleOrDefault(item => item.Id == id));
        public Task<Frame?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<bool> CodeExistsAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Frame frame, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
