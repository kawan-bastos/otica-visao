using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.LaboratoryOrders;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.LaboratoryOrders;

public sealed class LaboratoryOrderRepository(ApplicationDbContext context) : ILaboratoryOrderRepository
{
    public async Task<IReadOnlyList<LaboratoryOrder>> ListAsync(CancellationToken cancellationToken = default) =>
        await Query().AsNoTracking().OrderBy(order => order.Status == LaboratoryOrderStatus.Delivered)
            .ThenBy(order => order.ExpectedDeliveryDate).ThenByDescending(order => order.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task<LaboratoryOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query().SingleOrDefaultAsync(order => order.Id == id, cancellationToken);

    public Task<bool> ExistsForSaleItemAsync(Guid saleItemId, CancellationToken cancellationToken = default) =>
        context.LaboratoryOrders.AnyAsync(order => order.SaleItemId == saleItemId, cancellationToken);

    public Task AddAsync(LaboratoryOrder order, CancellationToken cancellationToken = default) =>
        context.LaboratoryOrders.AddAsync(order, cancellationToken).AsTask();

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);

    private IQueryable<LaboratoryOrder> Query() => context.LaboratoryOrders
        .Include(order => order.Sale).ThenInclude(sale => sale.Customer)
        .Include(order => order.SaleItem).Include(order => order.History);
}
