using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Sales;

public sealed class SaleRepository(ApplicationDbContext context) : ISaleRepository
{
    public async Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Sales.AsNoTracking().Include(sale => sale.Customer).Include(sale => sale.Items)
            .OrderByDescending(sale => sale.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Sales.Include(sale => sale.Customer).Include(sale => sale.Items)
            .SingleOrDefaultAsync(sale => sale.Id == id, cancellationToken);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) =>
        context.Sales.AddAsync(sale, cancellationToken).AsTask();

    public void Remove(Sale sale) => context.Sales.Remove(sale);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
