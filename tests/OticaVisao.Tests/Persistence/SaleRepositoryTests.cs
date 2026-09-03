using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Infrastructure.Sales;

namespace OticaVisao.Tests.Persistence;

public sealed class SaleRepositoryTests
{
    [Fact]
    public async Task RepositoryPersistsSaleAndLoadsCustomer()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"sales-{Guid.NewGuid()}").Options;
        await using var context = new ApplicationDbContext(options);
        var customer = new Customer(
            "Maria", "21999990000", "52998224725", new DateOnly(1990, 1, 1),
            new CustomerAddress("25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ"));
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        var repository = new SaleRepository(context);

        var sale = new Sale(customer.Id);
        await repository.AddAsync(sale);
        await repository.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var stored = await repository.GetByIdAsync(sale.Id);

        Assert.NotNull(stored);
        Assert.Equal("Maria", stored.Customer.Name);
        Assert.Single(await repository.ListAsync());
    }
}
