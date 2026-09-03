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

        var id = await service.CreateAsync(CreateRequest("Carlos"));

        Assert.Equal(id, Assert.Single(repository.Customers).Id);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task UpdateChangesExistingCustomer()
    {
        var customer = CreateCustomer("Carlos");
        var repository = new FakeCustomerRepository(customer);
        var service = new CustomerService(repository);

        await service.UpdateAsync(customer.Id, new UpdateCustomerRequest(
            "Carlos Silva", "21988880000", "52998224725", new DateOnly(1980, 1, 1),
            "25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ", "carlos@email.com", "Cliente antigo"));

        Assert.Equal("Carlos Silva", customer.Name);
        Assert.Equal("carlos@email.com", customer.Email);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task DeleteRemovesCustomerWithoutSales()
    {
        var customer = CreateCustomer("Carlos");
        var repository = new FakeCustomerRepository(customer);
        var service = new CustomerService(repository);

        await service.DeleteAsync(customer.Id);

        Assert.Empty(repository.Customers);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task DeleteRejectsCustomerWithSales()
    {
        var customer = CreateCustomer("Carlos");
        var repository = new FakeCustomerRepository(customer) { HasSales = true };
        var service = new CustomerService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(customer.Id));

        Assert.Single(repository.Customers);
        Assert.Equal(0, repository.SaveCount);
    }

    private static CreateCustomerRequest CreateRequest(string name) => new(
        name, "21999990000", "52998224725", new DateOnly(1980, 1, 1),
        "25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ", null, null);

    private static Customer CreateCustomer(string name) => new(
        name, "21999990000", "52998224725", new DateOnly(1980, 1, 1),
        new CustomerAddress("25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ"));

    private sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        public List<Customer> Customers { get; } = [.. customers];
        public int SaveCount { get; private set; }
        public bool HasSales { get; init; }

        public Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(Customers);
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.SingleOrDefault(customer => customer.Id == id));
        public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.SingleOrDefault(customer => customer.Cpf == cpf));
        public Task<bool> CpfExistsAsync(string cpf, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.Any(customer => customer.Cpf == cpf && customer.Id != excludingId));
        public Task<bool> HasSalesAsync(Guid customerId, CancellationToken cancellationToken = default) => Task.FromResult(HasSales);
        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            Customers.Add(customer);
            return Task.CompletedTask;
        }
        public Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            Customers.Remove(customer);
            return Task.CompletedTask;
        }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
