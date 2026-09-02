using OticaVisao.Application.Customers;
using OticaVisao.Domain.Customers;

namespace OticaVisao.Tests.Customers;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task CreatePersistsCustomer()
    {
        var repository = new FakeCustomerRepository();
        var service = new CustomerService(repository);

        var id = await service.CreateAsync(new CreateCustomerRequest("Carlos", "21999990000", null, null));

        Assert.Equal(id, Assert.Single(repository.Customers).Id);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task UpdateChangesExistingCustomer()
    {
        var customer = new Customer("Carlos", "21999990000");
        var repository = new FakeCustomerRepository(customer);
        var service = new CustomerService(repository);

        await service.UpdateAsync(customer.Id, new UpdateCustomerRequest("Carlos Silva", "21988880000", "carlos@email.com", "Cliente antigo"));

        Assert.Equal("Carlos Silva", customer.Name);
        Assert.Equal("carlos@email.com", customer.Email);
        Assert.Equal(1, repository.SaveCount);
    }

    private sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        public List<Customer> Customers { get; } = [.. customers];
        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(Customers);
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.SingleOrDefault(customer => customer.Id == id));
        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            Customers.Add(customer);
            return Task.CompletedTask;
        }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
