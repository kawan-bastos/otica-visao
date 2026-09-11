using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
using OticaVisao.Application.Reports;
using OticaVisao.Application.Costs;
using OticaVisao.Infrastructure.Costs;
using OticaVisao.Application.Auditing;
using OticaVisao.Infrastructure.Auditing;
using OticaVisao.Application.Engagement;
using OticaVisao.Infrastructure.Engagement;

namespace OticaVisao.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        Action<FrameImageStorageOptions>? configureImageStorage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddHttpContextAccessor();
        services.AddScoped<AdminAuditInterceptor>();
        services.AddDbContext<ApplicationDbContext>((provider, options) => options
            .UseNpgsql(connectionString)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning))
            .AddInterceptors(provider.GetRequiredService<AdminAuditInterceptor>()));
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
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
        services.AddScoped<PasswordHistoryService>();

        var authorization = services.AddAuthorizationBuilder()
            .AddPolicy(AdminAuthorization.Policy, policy =>
                policy.RequireRole(AdminAuthorization.Role))
            .AddPolicy(AdminAuthorization.GeneralPolicy, policy =>
                policy.RequireRole(AdminAuthorization.GeneralRole));
        foreach (var permission in AdminAuthorization.Permissions)
        {
            authorization.AddPolicy(permission.Policy, policy => policy.RequireAssertion(context =>
                context.User.IsInRole(AdminAuthorization.Role)
                && AdminAuthorization.HasPermission(context.User, permission.Value)));
        }

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
        services.AddOptions<LaboratoryDocumentStorageOptions>();
        services.AddSingleton<ILaboratoryDocumentStorage, LocalLaboratoryDocumentStorage>();
        services.AddScoped<IFrameRepository, FrameRepository>();
        services.AddScoped<FrameCatalogService>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<SaleService>();
        services.AddScoped<ILaboratoryOrderRepository, LaboratoryOrderRepository>();
        services.AddScoped<LaboratoryOrderService>();
        services.AddScoped<ReportService>();
        services.AddScoped<ICostRepository, CostRepository>();
        services.AddScoped<CostService>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<AuditLogService>();
        services.AddScoped<ICustomerEngagementService, CustomerEngagementService>();
        services.AddHostedService<ReservationExpirationService>();

        return services;
    }
}
