using OticaVisao.Application.Customers;
using OticaVisao.Application.Sales;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;
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
        var service = new SaleService(sales, new FakeCustomerRepository(customer), new FakeFrameRepository());

        var id = await service.CreateDraftAsync(customer.Id);

        Assert.Equal(id, Assert.Single(sales.Sales).Id);
        Assert.Equal(customer.Id, sales.Sales[0].CustomerId);
        Assert.Equal(1, sales.SaveCount);
    }

    [Fact]
    public async Task CreateDraftRejectsUnknownCustomer()
    {
        var service = new SaleService(new FakeSaleRepository(), new FakeCustomerRepository(), new FakeFrameRepository());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateDraftAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task AddAndRemoveItemReservesAndRestoresStock()
    {
        var customer = Customer();
        var frame = Frame();
        var sale = new Sale(customer.Id);
        var sales = new FakeSaleRepository { Sales = { sale } };
        var service = new SaleService(sales, new FakeCustomerRepository(customer), new FakeFrameRepository(frame));

        await service.AddItemAsync(sale.Id, new AddSaleItemRequest(
            frame.Id, 2, true, "Visão simples com antirreflexo", 300m, OpticalLaboratory.StandardOptical));

        var item = Assert.Single(sale.Items);
        Assert.Equal(1, frame.StockQuantity);
        Assert.Equal(678m, sale.Total);

        await service.RemoveItemAsync(sale.Id, item.Id);

        Assert.Empty(sale.Items);
        Assert.Equal(3, frame.StockQuantity);
    }

    private static Customer Customer() => new(
        "Maria", "21999990000", "52998224725", new DateOnly(1990, 1, 1),
        new CustomerAddress("25931-770", "Rua A", "10", null, "Piabetá", "Magé", "RJ"));

    private static Frame Frame() => new(
        "ARM-1", "Marca", "Modelo", "Preta", 219m, 3,
        FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);

    private sealed class FakeSaleRepository : ISaleRepository
    {
        public List<Sale> Sales { get; } = [];
        public int SaveCount { get; private set; }
        public Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Sale>>(Sales);
        public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Sales.SingleOrDefault(sale => sale.Id == id));
        public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) { Sales.Add(sale); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) { SaveCount++; return Task.CompletedTask; }
    }

    private sealed class FakeFrameRepository(params Frame[] frames) : IFrameRepository
    {
        public Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Frame>>(frames);
        public Task<IReadOnlyList<Frame>> ListPublicAsync(PublicFrameFilter filter, int? maximumItems = null, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Frame>>(frames);
        public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(frames.SingleOrDefault(frame => frame.Id == id));
        public Task<Frame?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<bool> CodeExistsAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Frame frame, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
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
