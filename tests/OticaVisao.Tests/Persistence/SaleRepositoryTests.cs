using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.Customers;
using OticaVisao.Domain.Sales;
using OticaVisao.Domain.Catalog;
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
        var frame = new Frame(
            "ARM-1", "Marca", "Modelo", "Preta", 219m, 2,
            FrameType.Prescription, FrameShape.Round, TargetAudience.Adult);
        context.Frames.Add(frame);
        await context.SaveChangesAsync();
        var repository = new SaleRepository(context);

        var sale = new Sale(customer.Id);
        sale.AddItem(frame, 1, true, "Visão simples", 300m, OpticalLaboratory.StandardOptical);
        await repository.AddAsync(sale);
        await repository.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var stored = await repository.GetByIdAsync(sale.Id);

        Assert.NotNull(stored);
        Assert.Equal("Maria", stored.Customer.Name);
        Assert.Equal(339m, Assert.Single(stored.Items).Total);
        Assert.Single(await repository.ListAsync());
    }
}
