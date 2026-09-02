using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Customers;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Tests.Persistence;

public sealed class CustomerRepositoryTests
{
    [Fact]
    public async Task RepositoryPersistsAndListsCustomersAlphabetically()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"customers-{Guid.NewGuid()}").Options;
        await using var context = new ApplicationDbContext(options);
        var repository = new CustomerRepository(context);
        await repository.AddAsync(new Customer("Zélia", "21999990000"));
        await repository.AddAsync(new Customer("Ana", "21988880000"));
        await repository.SaveChangesAsync();

        var customers = await repository.ListAsync(null);

        Assert.Collection(customers,
            first => Assert.Equal("Ana", first.Name),
            second => Assert.Equal("Zélia", second.Name));
        Assert.NotNull(await repository.GetByIdAsync(customers[0].Id));
    }
}
