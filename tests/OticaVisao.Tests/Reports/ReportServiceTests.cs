using OticaVisao.Application.Catalog;
using OticaVisao.Application.Reports;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Catalog;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Tests.Reports;

public sealed class ReportServiceTests
{
    [Fact]
    public async Task ReportCalculatesCompletedSalesAndLowStock()
    {
        var frame = Frame("ARM-1", 1);
        var sale = new Sale(Guid.NewGuid());
        sale.AddItem(frame, 2, true, "Visão simples", 300m, OpticalLaboratory.StandardOptical);
        sale.Complete(PaymentMethod.CreditCard, 5);
        var service = new ReportService(new FakeSaleRepository(sale), new FakeFrameRepository(frame));
        var today = DateOnly.FromDateTime(DateTime.Today);

        var report = await service.GetSalesReportAsync(today, today);

        Assert.Equal(1, report.CompletedSales);
        Assert.Equal(678m, report.Revenue);
        Assert.Equal(678m, report.AverageTicket);
        Assert.Equal(2, report.SoldUnits);
        Assert.Equal(2, report.CompleteGlasses);
        Assert.Equal(1, report.UniqueCustomers);
        Assert.Equal(PaymentMethod.CreditCard, Assert.Single(report.Payments).Method);
        Assert.Equal(2, Assert.Single(report.TopFrames).Quantity);
        Assert.Equal(1, Assert.Single(report.LowStock).StockQuantity);
    }

    [Fact]
    public async Task ReportIgnoresDraftSalesFromRevenue()
    {
        var draft = new Sale(Guid.NewGuid());
        var service = new ReportService(new FakeSaleRepository(draft), new FakeFrameRepository());
        var today = DateOnly.FromDateTime(DateTime.Today);

        var report = await service.GetSalesReportAsync(today, today);

        Assert.Equal(0, report.Revenue);
        Assert.Equal(1, report.DraftSales);
    }

    [Fact]
    public async Task ReportRejectsInvalidPeriod()
    {
        var service = new ReportService(new FakeSaleRepository(), new FakeFrameRepository());
        var today = DateOnly.FromDateTime(DateTime.Today);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSalesReportAsync(today, today.AddDays(-1)));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSalesReportAsync(today.AddDays(-367), today));
    }

    private static Frame Frame(string code, int stock) => new(code, "Marca", "Modelo", "Preta", 219m, stock,
        FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);

    private sealed class FakeSaleRepository(params Sale[] sales) : ISaleRepository
    {
        public Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Sale>>(sales);
        public Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(sales.SingleOrDefault(sale => sale.Id == id));
        public Task AddAsync(Sale sale, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(Sale sale) { }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
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
}
