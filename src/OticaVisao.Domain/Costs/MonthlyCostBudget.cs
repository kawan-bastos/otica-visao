namespace OticaVisao.Domain.Costs;

public sealed class MonthlyCostBudget
{
    private MonthlyCostBudget() { }
    public MonthlyCostBudget(DateOnly month, decimal amount) { Id = Guid.NewGuid(); Month = new DateOnly(month.Year, month.Month, 1); SetAmount(amount); }
    public Guid Id { get; private set; }
    public DateOnly Month { get; private set; }
    public decimal Amount { get; private set; }
    public void SetAmount(decimal amount) { ArgumentOutOfRangeException.ThrowIfNegative(amount); Amount = amount; }
}
