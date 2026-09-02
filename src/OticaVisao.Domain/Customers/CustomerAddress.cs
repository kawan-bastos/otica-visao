namespace OticaVisao.Domain.Customers;

public sealed record CustomerAddress
{
    private CustomerAddress() { }

    public CustomerAddress(string postalCode, string street, string number, string? complement, string neighborhood, string city, string state)
    {
        PostalCode = Required(postalCode, nameof(postalCode), 9);
        Street = Required(street, nameof(street), 160);
        Number = Required(number, nameof(number), 20);
        Complement = Optional(complement, nameof(complement), 80);
        Neighborhood = Required(neighborhood, nameof(neighborhood), 100);
        City = Required(city, nameof(city), 100);
        State = Required(state, nameof(state), 2).ToUpperInvariant();
    }

    public string PostalCode { get; private init; } = null!;
    public string Street { get; private init; } = null!;
    public string Number { get; private init; } = null!;
    public string? Complement { get; private init; }
    public string Neighborhood { get; private init; } = null!;
    public string City { get; private init; } = null!;
    public string State { get; private init; } = null!;

    private static string Required(string value, string parameterName, int maximumLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"O campo deve possuir no máximo {maximumLength} caracteres.", parameterName);
        return normalized;
    }

    private static string? Optional(string? value, string parameterName, int maximumLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Required(value, parameterName, maximumLength);
}
