using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using OticaVisao.Infrastructure;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Images;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", AdminAuthorization.Policy);
    options.Conventions.AllowAnonymousToPage("/Admin/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Admin/Account/AccessDenied");
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A conexão 'DefaultConnection' não foi configurada.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.Configure<AdminAccountOptions>(
    builder.Configuration.GetSection(AdminAccountOptions.SectionName));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "OticaVisao.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = 6 * 1024 * 1024);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
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
