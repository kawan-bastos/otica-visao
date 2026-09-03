using OticaVisao.Domain.Sales;

namespace OticaVisao.Tests.Sales;

public sealed class SaleTests
{
    [Fact]
    public void NewSaleStartsAsDraft()
    {
        var customerId = Guid.NewGuid();

        var sale = new Sale(customerId);

        Assert.NotEqual(Guid.Empty, sale.Id);
        Assert.Equal(customerId, sale.CustomerId);
        Assert.Equal(SaleStatus.Draft, sale.Status);
        Assert.Equal(sale.CreatedAtUtc, sale.UpdatedAtUtc);
    }

    [Fact]
    public void SaleRequiresCustomer() =>
        Assert.Throws<ArgumentException>(() => new Sale(Guid.Empty));
}
