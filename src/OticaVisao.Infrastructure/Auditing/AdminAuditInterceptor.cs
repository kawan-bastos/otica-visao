using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OticaVisao.Domain.Auditing;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Infrastructure.Auditing;

internal sealed class AdminAuditInterceptor(IHttpContextAccessor httpContextAccessor) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (context is null || httpContext is null
            || !httpContext.Request.Path.StartsWithSegments("/Admin")
            || httpContext.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var actor = httpContext.User.Identity.Name ?? "Administrador";
        var trackedEntries = context.ChangeTracker.Entries()
            .Where(entry => entry.Entity is Frame or Customer or Sale or LaboratoryOrder)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToArray();

        foreach (var entry in trackedEntries)
        {
            var (action, type, id, description) = Describe(entry);
            context.Add(new AuditLog(action, type, id, description, actor));
        }
    }

    private static (string Action, string Type, string Id, string Description) Describe(EntityEntry entry) => entry.Entity switch
    {
        Frame frame => (
            entry.State == EntityState.Added ? "Armação cadastrada" : entry.State == EntityState.Deleted ? "Armação excluída" : "Armação alterada",
            "Armação", frame.Id.ToString(), $"{frame.Code} — {frame.Brand} {frame.Model}"),
        Customer customer => (
            entry.State == EntityState.Added ? "Cliente cadastrado" : entry.State == EntityState.Deleted ? "Cliente excluído" : "Cliente alterado",
            "Cliente", customer.Id.ToString(), customer.Name),
        Sale sale => DescribeSale(entry, sale),
        LaboratoryOrder order => (
            entry.State == EntityState.Added ? "Pedido de lentes criado" : entry.State == EntityState.Deleted ? "Pedido de lentes excluído" : "Pedido de lentes atualizado",
            "Pedido de lentes", order.Id.ToString(), $"Pedido da venda {order.SaleId}"),
        _ => throw new InvalidOperationException("Entidade não suportada pela auditoria.")
    };

    private static (string Action, string Type, string Id, string Description) DescribeSale(EntityEntry entry, Sale sale)
    {
        var action = entry.State switch
        {
            EntityState.Added => "Venda iniciada",
            EntityState.Deleted => "Venda excluída",
            _ when StatusChangedTo(entry, SaleStatus.Completed) => "Venda concluída",
            _ when StatusChangedTo(entry, SaleStatus.Cancelled) => "Venda cancelada",
            _ when StatusChangedTo(entry, SaleStatus.Reversed) => "Venda estornada",
            _ => "Venda alterada"
        };
        return (action, "Venda", sale.Id.ToString(), $"Venda do cliente {sale.CustomerId}");
    }

    private static bool StatusChangedTo(EntityEntry entry, SaleStatus status)
    {
        var property = entry.Property(nameof(Sale.Status));
        return property.IsModified && property.CurrentValue is SaleStatus current && current == status;
    }
}
