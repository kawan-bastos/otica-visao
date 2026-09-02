using Microsoft.AspNetCore.Http;
using OticaVisao.Infrastructure;
using OticaVisao.Infrastructure.Authentication;

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
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program;
