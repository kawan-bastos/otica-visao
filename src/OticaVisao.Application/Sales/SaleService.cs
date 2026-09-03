using OticaVisao.Application.Customers;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed class SaleService(ISaleRepository saleRepository, ICustomerRepository customerRepository)
{
    public async Task<IReadOnlyList<SaleListItem>> ListAsync(CancellationToken cancellationToken = default) =>
        (await saleRepository.ListAsync(cancellationToken)).Select(ToListItem).ToArray();

    public async Task<SaleListItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(id, cancellationToken);
        return sale is null ? null : ToListItem(sale);
    }

    public async Task<Guid> CreateDraftAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (await customerRepository.GetByIdAsync(customerId, cancellationToken) is null)
            throw new KeyNotFoundException("Cliente não encontrado.");

        var sale = new Sale(customerId);
        await saleRepository.AddAsync(sale, cancellationToken);
        await saleRepository.SaveChangesAsync(cancellationToken);
        return sale.Id;
    }

    private static SaleListItem ToListItem(Sale sale) => new(
        sale.Id, sale.CustomerId, sale.Customer.Name, sale.Customer.Phone,
        sale.Status, sale.CreatedAtUtc, sale.UpdatedAtUtc);
}
