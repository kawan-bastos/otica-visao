using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Customers;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Infrastructure.Authentication;

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
        await repository.AddAsync(CreateCustomer("Zélia", "52998224725"));
        await repository.AddAsync(CreateCustomer("Ana", "11144477735"));
        await repository.SaveChangesAsync();

        var customers = await repository.ListAsync(null);

        Assert.Collection(customers,
            first => Assert.Equal("Ana", first.Name),
            second => Assert.Equal("Zélia", second.Name));
        Assert.NotNull(await repository.GetByIdAsync(customers[0].Id));
        Assert.True(await repository.CpfExistsAsync("11144477735"));
    }

    [Fact]
    public async Task DeleteRemovesCustomerAndLinkedAccountTogether()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"customer-delete-{Guid.NewGuid()}").Options;
        await using var context = new ApplicationDbContext(options);
        var account = new ApplicationUser { Id = Guid.NewGuid(), UserName = "cliente@email.com", DisplayName = "Cliente" };
        var customer = CreateCustomer("Cliente", "52998224725");
        customer.LinkToAccount(account.Id);
        context.Users.Add(account);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        var repository = new CustomerRepository(context);

        await repository.DeleteAsync(customer);
        await repository.SaveChangesAsync();

        Assert.Empty(context.Customers);
        Assert.Empty(context.Users);
    }

    private static Customer CreateCustomer(string name, string cpf) => new(
        name, "21999990000", cpf, new DateOnly(1990, 1, 1),
        new CustomerAddress("25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ"));
}
