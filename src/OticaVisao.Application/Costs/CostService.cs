using OticaVisao.Domain.Costs;
namespace OticaVisao.Application.Costs;
public sealed class CostService(ICostRepository repository)
{
    public async Task<CostReport> GetAsync(DateOnly month, CancellationToken ct = default)
    {
        month = new(month.Year, month.Month, 1); var end = month.AddMonths(1).AddDays(-1);
        var previousStart = month.AddMonths(-1); var previousEnd = month.AddDays(-1);
        var current = await repository.ListAsync(month, end, ct); var previous = await repository.ListAsync(previousStart, previousEnd, ct);
        var total = current.Sum(x => x.Amount); var previousTotal = previous.Sum(x => x.Amount);
        var today = DateOnly.FromDateTime(DateTime.Today); var elapsed = month.Year == today.Year && month.Month == today.Month ? today.Day : end.Day;
        var forecast = elapsed == 0 ? total : total / elapsed * end.Day; var budget = (await repository.GetBudgetAsync(month, ct))?.Amount ?? 0;
        var categories = Enum.GetValues<CostCategory>().Select(category => new CostCategoryReport(category,
            current.Where(x => x.Category == category).Sum(x => x.Amount), previous.Where(x => x.Category == category).Sum(x => x.Amount),
            total == 0 ? 0 : current.Where(x => x.Category == category).Sum(x => x.Amount) / total * 100)).ToArray();
        return new(month, total, previousTotal, forecast, budget, budget == 0 ? 0 : total / budget * 100, categories);
    }
    public async Task AddAsync(CostCategory category, decimal amount, DateOnly date, string description, CancellationToken ct = default) { await repository.AddAsync(new(category, amount, date, description), ct); await repository.SaveChangesAsync(ct); }
    public async Task SetBudgetAsync(DateOnly month, decimal amount, CancellationToken ct = default) { month = new(month.Year, month.Month, 1); var budget = await repository.GetBudgetAsync(month, ct); if (budget is null) await repository.AddBudgetAsync(new(month, amount), ct); else budget.SetAmount(amount); await repository.SaveChangesAsync(ct); }
}
