using OticaVisao.Application.Customers;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Tests.Sales;

public sealed class SaleServiceTests
{
    [Fact]
    public async Task CreateDraftPersistsSaleForExistingCustomer()
    {
        var customer = Customer();
        var sales = new FakeSaleRepository();
        var service = new SaleService(sales, new FakeCustomerRepository(customer));

        var id = await service.CreateDraftAsync(customer.Id);

        Assert.Equal(id, Assert.Single(sales.Sales).Id);
        Assert.Equal(customer.Id, sales.Sales[0].CustomerId);
        Assert.Equal(1, sales.SaveCount);
    }

    [Fact]
    public async Task CreateDraftRejectsUnknownCustomer()
    {
        var service = new SaleService(new FakeSaleRepository(), new FakeCustomerRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateDraftAsync(Guid.NewGuid()));
    }

    private static Customer Customer() => new(
        "Maria", "21999990000", "52998224725", new DateOnly(1990, 1, 1),
        new CustomerAddress("25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ"));

    private sealed class FakeSaleRepository : ISaleRepository
    {
        public List<Sale> Sales { get; } = [];
        public int SaveCount { get; private set; }
        public Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Sale>>(Sales);
        public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Sales.SingleOrDefault(sale => sale.Id == id));
        public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) { Sales.Add(sale); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.CompletedTask; }
    }

    private sealed class FakeCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(customers.SingleOrDefault(customer => customer.Id == id));
        public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default) => Task.FromResult(customers.SingleOrDefault(customer => customer.Cpf == cpf));
        public Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Customer>>(customers);
        public Task<bool> CpfExistsAsync(string cpf, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Customer customer, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
