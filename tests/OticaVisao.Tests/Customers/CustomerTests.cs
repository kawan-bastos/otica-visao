using OticaVisao.Domain.Customers;

namespace OticaVisao.Tests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void CustomerNormalizesBasicData()
    {
        var customer = CreateCustomer(" Maria Silva ", " (21) 99999-0000 ", " maria@email.com ", " Prefere armações leves. ");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Maria Silva", customer.Name);
        Assert.Equal("(21) 99999-0000", customer.Phone);
        Assert.Equal("52998224725", customer.Cpf);
        Assert.Equal(new DateOnly(1990, 5, 10), customer.BirthDate);
        Assert.Equal("Piabetá", customer.Address?.Neighborhood);
        Assert.Equal("maria@email.com", customer.Email);
        Assert.Equal("Prefere armações leves.", customer.Notes);
    }

    [Fact]
    public void CustomerRequiresNameAndPhone()
    {
        Assert.Throws<ArgumentException>(() => CreateCustomer("", "(21) 99999-0000"));
        Assert.Throws<ArgumentException>(() => CreateCustomer("Maria", ""));
    }

    [Fact]
    public void UpdateChangesContactInformation()
    {
        var customer = CreateCustomer("Maria", "(21) 99999-0000");

        customer.Update("Maria Souza", "(21) 98888-0000", "52998224725", new DateOnly(1990, 5, 10), Address(), null, "Retornar em dezembro.");

        Assert.Equal("Maria Souza", customer.Name);
        Assert.Equal("(21) 98888-0000", customer.Phone);
        Assert.Null(customer.Email);
        Assert.Equal("Retornar em dezembro.", customer.Notes);
        Assert.True(customer.UpdatedAtUtc >= customer.CreatedAtUtc);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("123.456.789-00")]
    public void CustomerRejectsInvalidCpf(string cpf)
    {
        Assert.Throws<ArgumentException>(() => new Customer("Maria", "21999990000", cpf, new DateOnly(1990, 5, 10), Address()));
    }

    private static Customer CreateCustomer(string name, string phone, string? email = null, string? notes = null) =>
        new(name, phone, "529.982.247-25", new DateOnly(1990, 5, 10), Address(), email, notes);

    private static CustomerAddress Address() =>
        new("25931-770", "Rua Arthur Rodrigues Loivos", "370", null, "Piabetá", "Magé", "RJ");
}
