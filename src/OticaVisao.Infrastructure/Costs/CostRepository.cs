using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Costs;
using OticaVisao.Domain.Costs;
using OticaVisao.Infrastructure.Persistence;
namespace OticaVisao.Infrastructure.Costs;
public sealed class CostRepository(ApplicationDbContext context) : ICostRepository
{
    public async Task<IReadOnlyList<BusinessCost>> ListAsync(DateOnly from, DateOnly through, CancellationToken ct = default) => await context.BusinessCosts.AsNoTracking().Where(x => x.IncurredOn >= from && x.IncurredOn <= through).OrderBy(x => x.IncurredOn).ToArrayAsync(ct);
    public Task<MonthlyCostBudget?> GetBudgetAsync(DateOnly month, CancellationToken ct = default) => context.MonthlyCostBudgets.SingleOrDefaultAsync(x => x.Month == month, ct);
    public Task AddAsync(BusinessCost cost, CancellationToken ct = default) => context.BusinessCosts.AddAsync(cost, ct).AsTask();
    public Task AddBudgetAsync(MonthlyCostBudget budget, CancellationToken ct = default) => context.MonthlyCostBudgets.AddAsync(budget, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
