using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using OticaVisao.Infrastructure;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Images;
using OticaVisao.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using OticaVisao.Web.Configuration;
using OticaVisao.Web.Models;
using System.Globalization;
using Microsoft.AspNetCore.DataProtection;
using OticaVisao.Web.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

if (!builder.Environment.IsDevelopment())
{
    ProductionConfigurationValidator.Validate(builder.Configuration);
}

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AdminAuthorization.Policy);
    options.Conventions.AuthorizeFolder("/Admin/Frames", AdminAuthorization.FramesPolicy);
    options.Conventions.AuthorizeFolder("/Admin/Customers", AdminAuthorization.CustomersPolicy);
    options.Conventions.AuthorizeFolder("/Admin/Sales", AdminAuthorization.SalesPolicy);
    options.Conventions.AuthorizeFolder("/Admin/Reservations", AdminAuthorization.ReservationsPolicy);
    options.Conventions.AuthorizeFolder("/Admin/LaboratoryOrders", AdminAuthorization.LaboratoryOrdersPolicy);
    options.Conventions.AuthorizeFolder("/Admin/Reports", AdminAuthorization.ReportsPolicy);
    options.Conventions.AuthorizeFolder("/Admin/Audit", AdminAuthorization.AuditPolicy);
    options.Conventions.AllowAnonymousToPage("/Admin/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Admin/Account/AccessDenied");
}).AddMvcOptions(options => options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider()));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A conexão 'DefaultConnection' não foi configurada.");

builder.Services.AddInfrastructure(connectionString, options =>
    builder.Configuration.GetSection(FrameImageStorageOptions.SectionName).Bind(options));
builder.Services.Configure<LaboratoryDocumentStorageOptions>(
    builder.Configuration.GetSection(LaboratoryDocumentStorageOptions.SectionName));
builder.Services.Configure<AdminAccountOptions>(
    builder.Configuration.GetSection(AdminAccountOptions.SectionName));
builder.Services.Configure<AccountEmailOptions>(
    builder.Configuration.GetSection(AccountEmailOptions.SectionName));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IAccountEmailSender, SmtpAccountEmailSender>();
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("OticaVisao");
var dataProtectionPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "data-protection-keys")
    : builder.Configuration["DataProtection:KeysPath"]!;
dataProtection.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OticaVisao.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        var redirectUri = context.Request.Path.StartsWithSegments("/Account")
            ? context.RedirectUri.Replace(options.LoginPath.Value!, "/Account/Login", StringComparison.Ordinal)
            : context.RedirectUri;
        context.Response.Redirect(redirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = 6 * 1024 * 1024);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var request = context.Request;
        var client = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var authenticatedClient = context.User.Identity?.IsAuthenticated == true
            ? $"user:{context.User.Identity.Name}"
            : $"ip:{client}";

        if (HttpMethods.IsPost(request.Method)
            && request.Path.Equals("/Account/Register", StringComparison.OrdinalIgnoreCase))
            return RateLimitPartition.GetFixedWindowLimiter($"register:{client}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            });

        if (HttpMethods.IsPost(request.Method)
            && (request.Path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase)
                || request.Path.Equals("/Admin/Account/Login", StringComparison.OrdinalIgnoreCase)
                || request.Path.Equals("/Account/ForgotPassword", StringComparison.OrdinalIgnoreCase)
                || request.Path.Equals("/Account/ResetPassword", StringComparison.OrdinalIgnoreCase)))
            return RateLimitPartition.GetFixedWindowLimiter($"login:{client}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            });

        if (request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
            return RateLimitPartition.GetFixedWindowLimiter($"health:{client}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });

        if (request.Path.StartsWithSegments("/frame-images"))
            return RateLimitPartition.GetFixedWindowLimiter($"images:{client}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });

        if (HttpMethods.IsGet(request.Method)
            && (request.Path.Equals("/", StringComparison.OrdinalIgnoreCase)
                || request.Path.StartsWithSegments("/Frames")))
            return RateLimitPartition.GetFixedWindowLimiter($"catalog:{client}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });

        if (HttpMethods.IsPost(request.Method)
            && (request.Path.StartsWithSegments("/Admin") || request.Path.StartsWithSegments("/Account")))
            return RateLimitPartition.GetFixedWindowLimiter($"write:{authenticatedClient}", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });

        return RateLimitPartition.GetNoLimiter("unrestricted");
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync(
            "Muitas tentativas em pouco tempo. Aguarde alguns minutos e tente novamente.", cancellationToken);
    };
});

var app = builder.Build();

var brazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(brazilianCulture),
    SupportedCultures = [brazilianCulture],
    SupportedUICultures = [brazilianCulture]
});

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await database.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/pagina-nao-encontrada");

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(self), microphone=(), geolocation=()";

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; " +
            "connect-src 'self' https://viacep.com.br; " +
            "frame-src https://www.google.com; object-src 'none'; base-uri 'self'; " +
            "form-action 'self'; frame-ancestors 'none'";
    }

    await next();
});

app.UseRouting();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Admin")
        || context.Request.Path.StartsWithSegments("/Account"))
    {
        context.Response.Headers.CacheControl = "no-store, no-cache";
        context.Response.Headers.Pragma = "no-cache";
    }
    await next();
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapGet("/health", async (ApplicationDbContext database, CancellationToken cancellationToken) =>
{
    try
    {
        return await database.Database.CanConnectAsync(cancellationToken)
            ? Results.Ok(new { status = "healthy" })
            : Results.Problem("Banco de dados indisponível.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch
    {
        return Results.Problem("Banco de dados indisponível.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();
app.MapGet("/frame-images/{fileName}", async (
    string fileName,
    IFrameImageStorage storage,
    CancellationToken cancellationToken) =>
{
    var image = await storage.OpenReadAsync(fileName, cancellationToken);
    return image is null
        ? Results.NotFound()
        : Results.Stream(image.Content, image.ContentType, enableRangeProcessing: true);
}).AllowAnonymous();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program;
