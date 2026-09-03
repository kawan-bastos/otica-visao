using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OticaVisao.Application.Catalog;
using OticaVisao.Infrastructure.Catalog;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Infrastructure.Images;
using OticaVisao.Application.Customers;
using OticaVisao.Infrastructure.Customers;
using OticaVisao.Application.Sales;
using OticaVisao.Infrastructure.Sales;
using OticaVisao.Application.LaboratoryOrders;
using OticaVisao.Infrastructure.LaboratoryOrders;

namespace OticaVisao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<FrameImageStorageOptions>? configureImageStorage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthorizationBuilder()
            .AddPolicy(AdminAuthorization.Policy, policy =>
                policy.RequireRole(AdminAuthorization.Role));

        services.AddScoped<AdminAccountSeeder>();
        services.AddHostedService<AdminAccountInitializationService>();
        if (configureImageStorage is not null)
        {
            services.Configure(configureImageStorage);
        }
        else
        {
            services.Configure<FrameImageStorageOptions>(_ => { });
        }

        services.AddSingleton<IFrameImageStorage, LocalFrameImageStorage>();
        services.AddScoped<IFrameRepository, FrameRepository>();
        services.AddScoped<FrameCatalogService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<SaleService>();
        services.AddScoped<ILaboratoryOrderRepository, LaboratoryOrderRepository>();
        services.AddScoped<LaboratoryOrderService>();

        return services;
    }
}
