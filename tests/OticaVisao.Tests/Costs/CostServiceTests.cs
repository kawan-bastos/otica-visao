using OticaVisao.Application.Costs;
using OticaVisao.Domain.Costs;

namespace OticaVisao.Tests.Costs;

public sealed class CostServiceTests
{
    [Fact]
    public async Task ReportCalculatesTotalsBudgetAndCategoryShare()
    {
        var month = new DateOnly(2026, 9, 1);
        var repository = new FakeCostRepository(
            new BusinessCost(CostCategory.Frames, 100m, month, "Armações"),
            new BusinessCost(CostCategory.Rent, 50m, month.AddDays(1), "Aluguel"),
            new BusinessCost(CostCategory.Frames, 200m, month.AddMonths(-1), "Armações anteriores"));
        await repository.AddBudgetAsync(new MonthlyCostBudget(month, 300m));
        var service = new CostService(repository);

        var report = await service.GetAsync(month);

        Assert.Equal(150m, report.MonthToDate);
        Assert.Equal(200m, report.PreviousMonth);
        Assert.Equal(300m, report.Budget);
        Assert.Equal(50m, report.BudgetUsed);
        var frames = Assert.Single(report.Categories, x => x.Category == CostCategory.Frames);
        Assert.Equal(100m, frames.Current);
        Assert.Equal(200m, frames.Previous);
        Assert.Equal(100m / 150m * 100m, frames.Share);
    }

    [Fact]
    public async Task SetBudgetUpdatesExistingMonthInsteadOfDuplicatingIt()
    {
        var month = new DateOnly(2026, 9, 1);
        var repository = new FakeCostRepository();
        var service = new CostService(repository);

        await service.SetBudgetAsync(month, 500m);
        await service.SetBudgetAsync(month, 750m);

        Assert.Single(repository.Budgets);
        Assert.Equal(750m, repository.Budgets[0].Amount);
    }

    private sealed class FakeCostRepository(params BusinessCost[] costs) : ICostRepository
    {
        public List<MonthlyCostBudget> Budgets { get; } = [];

        public Task<IReadOnlyList<BusinessCost>> ListAsync(DateOnly from, DateOnly through, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<BusinessCost>>(costs.Where(x => x.IncurredOn >= from && x.IncurredOn <= through).ToArray());

        public Task<MonthlyCostBudget?> GetBudgetAsync(DateOnly month, CancellationToken ct = default) =>
            Task.FromResult(Budgets.SingleOrDefault(x => x.Month == month));

        public Task AddAsync(BusinessCost cost, CancellationToken ct = default) => Task.CompletedTask;

        public Task AddBudgetAsync(MonthlyCostBudget budget, CancellationToken ct = default)
        {
            Budgets.Add(budget);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
