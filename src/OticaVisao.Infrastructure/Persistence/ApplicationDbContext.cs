using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Auditing;
using OticaVisao.Domain.Engagement;
using OticaVisao.Domain.Costs;

namespace OticaVisao.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Frame> Frames => Set<Frame>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<LaboratoryOrder> LaboratoryOrders => Set<LaboratoryOrder>();
    public DbSet<LaboratoryOrderHistory> LaboratoryOrderHistory => Set<LaboratoryOrderHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FrameFavorite> FrameFavorites => Set<FrameFavorite>();
    public DbSet<FrameReservation> FrameReservations => Set<FrameReservation>();
    public DbSet<BusinessCost> BusinessCosts => Set<BusinessCost>();
    public DbSet<MonthlyCostBudget> MonthlyCostBudgets => Set<MonthlyCostBudget>();
    public DbSet<PasswordHistoryEntry> PasswordHistory => Set<PasswordHistoryEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
