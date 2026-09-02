using OticaVisao.Domain.Customers;

namespace OticaVisao.Tests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void CustomerNormalizesBasicData()
    {
        var customer = new Customer(" Maria Silva ", " (21) 99999-0000 ", " maria@email.com ", " Prefere armações leves. ");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Maria Silva", customer.Name);
        Assert.Equal("(21) 99999-0000", customer.Phone);
        Assert.Equal("maria@email.com", customer.Email);
        Assert.Equal("Prefere armações leves.", customer.Notes);
    }

    [Fact]
    public void CustomerRequiresNameAndPhone()
    {
        Assert.Throws<ArgumentException>(() => new Customer("", "(21) 99999-0000"));
        Assert.Throws<ArgumentException>(() => new Customer("Maria", ""));
    }

    [Fact]
    public void UpdateChangesContactInformation()
    {
        var customer = new Customer("Maria", "(21) 99999-0000");

        customer.Update("Maria Souza", "(21) 98888-0000", null, "Retornar em dezembro.");

        Assert.Equal("Maria Souza", customer.Name);
        Assert.Equal("(21) 98888-0000", customer.Phone);
        Assert.Null(customer.Email);
        Assert.Equal("Retornar em dezembro.", customer.Notes);
        Assert.True(customer.UpdatedAtUtc >= customer.CreatedAtUtc);
    }
}
