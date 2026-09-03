namespace OticaVisao.Domain.Customers;

public sealed class Customer
{
    private Customer()
    {
        Name = null!;
        Phone = null!;
    }

    public Customer(string name, string phone, string cpf, DateOnly birthDate, CustomerAddress address, string? email = null, string? notes = null)
    {
        Id = Guid.NewGuid();
        Name = RequiredText(name, nameof(name), 120);
        Phone = RequiredText(phone, nameof(phone), 20);
        Cpf = BrazilianCpf.Normalize(cpf);
        BirthDate = ValidBirthDate(birthDate);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = OptionalText(email, nameof(email), 160);
        Notes = OptionalText(notes, nameof(notes), 1000);
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string? Cpf { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public CustomerAddress? Address { get; private set; }
    public Guid? AccountUserId { get; private set; }
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Update(string name, string phone, string cpf, DateOnly birthDate, CustomerAddress address, string? email, string? notes)
    {
        Name = RequiredText(name, nameof(name), 120);
        Phone = RequiredText(phone, nameof(phone), 20);
        Cpf = BrazilianCpf.Normalize(cpf);
        BirthDate = ValidBirthDate(birthDate);
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Email = OptionalText(email, nameof(email), 160);
        Notes = OptionalText(notes, nameof(notes), 1000);
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void LinkToAccount(Guid accountUserId)
    {
        if (accountUserId == Guid.Empty) throw new ArgumentException("Informe uma conta válida.", nameof(accountUserId));
        if (AccountUserId.HasValue && AccountUserId != accountUserId)
            throw new InvalidOperationException("Esta ficha já está vinculada a outra conta.");
        AccountUserId = accountUserId;
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

    private static DateOnly ValidBirthDate(DateOnly value)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (value > today || value < today.AddYears(-120))
            throw new ArgumentOutOfRangeException(nameof(value), "Informe uma data de nascimento válida.");
        return value;
    }
}
