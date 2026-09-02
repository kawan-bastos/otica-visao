namespace OticaVisao.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
        Name = null!;
        Phone = null!;
    }

    public Customer(string name, string phone, string? email = null, string? notes = null)
    {
        Id = Guid.NewGuid();
        Name = RequiredText(name, nameof(name), 120);
        Phone = RequiredText(phone, nameof(phone), 20);
        Email = OptionalText(email, nameof(email), 160);
        Notes = OptionalText(notes, nameof(notes), 1000);
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Update(string name, string phone, string? email, string? notes)
    {
        Name = RequiredText(name, nameof(name), 120);
        Phone = RequiredText(phone, nameof(phone), 20);
        Email = OptionalText(email, nameof(email), 160);
        Notes = OptionalText(notes, nameof(notes), 1000);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    private static string RequiredText(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException($"O campo deve possuir no máximo {maximumLength} caracteres.", parameterName);
        }
        return normalized;
    }

    private static string? OptionalText(string? value, string parameterName, int maximumLength) =>
        string.IsNullOrWhiteSpace(value) ? null : RequiredText(value, parameterName, maximumLength);
}
