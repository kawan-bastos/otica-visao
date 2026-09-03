using OticaVisao.Application.Customers;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Sales;
using OticaVisao.Application.LaboratoryOrders;

namespace OticaVisao.Application.Sales;

public sealed class SaleService(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository,
    IFrameRepository frameRepository,
    ILaboratoryOrderRepository laboratoryOrderRepository)
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

    public async Task CompleteAsync(Guid saleId, PaymentMethod paymentMethod, int installments, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        sale.Complete(paymentMethod, installments);
        foreach (var item in sale.Items.Where(item => item.IncludesLenses))
            await laboratoryOrderRepository.AddAsync(new OticaVisao.Domain.LaboratoryOrders.LaboratoryOrder(sale, item), cancellationToken);
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        if (sale.Status != SaleStatus.Draft)
            throw new InvalidOperationException("Somente vendas em andamento podem ser canceladas.");

        foreach (var item in sale.Items)
        {
            var frame = await frameRepository.GetByIdAsync(item.FrameId, cancellationToken)
                ?? throw new InvalidOperationException($"A armação {item.FrameCode} não foi encontrada; a venda não pode ser cancelada automaticamente.");
            frame.AddToStock(item.Quantity);
        }

        sale.Cancel();
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCancelledAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        if (sale.Status != SaleStatus.Cancelled)
            throw new InvalidOperationException("Somente vendas canceladas podem ser excluídas do histórico.");

        saleRepository.Remove(sale);
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReverseAsync(Guid saleId, string reason, string reversedBy, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        if (sale.Status != SaleStatus.Completed) throw new InvalidOperationException("Somente vendas concluídas podem ser estornadas.");

        var frames = new List<(OticaVisao.Domain.Catalog.Frame Frame, int Quantity)>();
        foreach (var item in sale.Items)
        {
            var frame = await frameRepository.GetByIdAsync(item.FrameId, cancellationToken)
                ?? throw new InvalidOperationException($"A armação {item.FrameCode} não foi encontrada; o estorno não pode ser concluído automaticamente.");
            frames.Add((frame, item.Quantity));
        }

        sale.Reverse(reason, reversedBy);
        foreach (var entry in frames) entry.Frame.AddToStock(entry.Quantity);
        foreach (var order in await laboratoryOrderRepository.ListBySaleIdAsync(saleId, cancellationToken)) order.Cancel();
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteReversedAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await saleRepository.GetByIdAsync(saleId, cancellationToken)
            ?? throw new KeyNotFoundException("Venda não encontrada.");
        if (sale.Status != SaleStatus.Reversed) throw new InvalidOperationException("Somente vendas estornadas podem ser excluídas permanentemente.");

        laboratoryOrderRepository.RemoveRange(await laboratoryOrderRepository.ListBySaleIdAsync(saleId, cancellationToken));
        saleRepository.Remove(sale);
        await saleRepository.SaveChangesAsync(cancellationToken);
    }

    private static SaleListItem ToListItem(Sale sale) => new(
        sale.Id, sale.CustomerId, sale.Customer.Name, sale.Customer.Phone,
        sale.Status, sale.PaymentMethod, sale.Installments, sale.Items.Select(item => new SaleItemListItem(
            item.Id, item.FrameId, item.FrameCode, item.FrameBrand, item.FrameModel, item.FrameColor,
            item.Quantity, item.IncludesLenses, item.FrameUnitPrice, item.LensDescription,
            item.LensUnitPrice, item.Laboratory, item.Total)).ToArray(),
        sale.Total, sale.FinalTotal, sale.CompletedAtUtc, sale.CancelledAtUtc, sale.ReversedAtUtc, sale.ReversalReason, sale.ReversedBy, sale.CreatedAtUtc, sale.UpdatedAtUtc);
}
