namespace OticaVisao.Domain.Costs;

public sealed class BusinessCost
{
    private BusinessCost() { }
    public BusinessCost(CostCategory category, decimal amount, DateOnly incurredOn, string description)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Id = Guid.NewGuid(); Category = category; Amount = amount; IncurredOn = incurredOn;
        Description = string.IsNullOrWhiteSpace(description) ? throw new ArgumentException("Informe a descrição.", nameof(description)) : description.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id { get; private set; }
    public CostCategory Category { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly IncurredOn { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
}
