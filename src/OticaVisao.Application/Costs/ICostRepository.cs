using OticaVisao.Domain.Costs;
namespace OticaVisao.Application.Costs;
public interface ICostRepository
{
    Task<IReadOnlyList<BusinessCost>> ListAsync(DateOnly from, DateOnly through, CancellationToken ct = default);
    Task<MonthlyCostBudget?> GetBudgetAsync(DateOnly month, CancellationToken ct = default);
    Task AddAsync(BusinessCost cost, CancellationToken ct = default);
    Task AddBudgetAsync(MonthlyCostBudget budget, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
