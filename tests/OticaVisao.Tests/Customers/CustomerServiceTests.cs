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

        public Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(Customers);
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.SingleOrDefault(customer => customer.Id == id));
        public Task<bool> CpfExistsAsync(string cpf, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(Customers.Any(customer => customer.Cpf == cpf && customer.Id != excludingId));
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
