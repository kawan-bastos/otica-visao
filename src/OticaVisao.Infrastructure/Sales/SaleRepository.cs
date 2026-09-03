using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Sales;

public sealed class SaleRepository(ApplicationDbContext context) : ISaleRepository
{
    public async Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Sales.AsNoTracking().Include(sale => sale.Customer)
            .OrderByDescending(sale => sale.CreatedAtUtc).ToListAsync(cancellationToken);

    public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Sales.AsNoTracking().Include(sale => sale.Customer)
            .SingleOrDefaultAsync(sale => sale.Id == id, cancellationToken);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) =>
        context.Sales.AddAsync(sale, cancellationToken).AsTask();

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
