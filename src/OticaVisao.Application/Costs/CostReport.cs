using OticaVisao.Domain.Costs;
namespace OticaVisao.Application.Costs;
public sealed record CostCategoryReport(CostCategory Category, decimal Current, decimal Previous, decimal Share);
public sealed record CostReport(DateOnly Month, decimal MonthToDate, decimal PreviousMonth, decimal Forecast, decimal Budget, decimal BudgetUsed, IReadOnlyList<CostCategoryReport> Categories);
