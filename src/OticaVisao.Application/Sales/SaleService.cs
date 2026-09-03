using OticaVisao.Application.Customers;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public sealed class SaleService(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository,
    IFrameRepository frameRepository)
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

    public async Task AddItemAsync(Guid saleId, AddSaleItemRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        var frame = await frameRepository.GetByIdAsync(request.FrameId, cancellationToken)
            ?? throw new KeyNotFoundException("Armação não encontrada.");
        if (!frame.IsActive) throw new InvalidOperationException("Esta armação está desativada.");

        frame.RemoveFromStock(request.Quantity);
        sale.AddItem(frame, request.Quantity, request.IncludesLenses, request.LensDescription, request.LensUnitPrice, request.Laboratory);
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(Guid saleId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        var item = sale.RemoveItem(itemId);
        var frame = await frameRepository.GetByIdAsync(item.FrameId, cancellationToken)
            ?? throw new InvalidOperationException("A armação deste item não foi encontrada.");
        frame.AddToStock(item.Quantity);
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    private static SaleListItem ToListItem(Sale sale) => new(
        sale.Id, sale.CustomerId, sale.Customer.Name, sale.Customer.Phone,
        sale.Status, sale.Items.Select(item => new SaleItemListItem(
            item.Id, item.FrameId, item.FrameCode, item.FrameBrand, item.FrameModel, item.FrameColor,
            item.Quantity, item.IncludesLenses, item.FrameUnitPrice, item.LensDescription,
            item.LensUnitPrice, item.Laboratory, item.Total)).ToArray(),
        sale.Total, sale.CreatedAtUtc, sale.UpdatedAtUtc);
}
