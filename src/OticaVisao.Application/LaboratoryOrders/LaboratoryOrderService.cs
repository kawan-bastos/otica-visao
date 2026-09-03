using OticaVisao.Application.Sales;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.LaboratoryOrders;

public sealed class LaboratoryOrderService(ILaboratoryOrderRepository repository, ISaleRepository saleRepository)
{
    public async Task<IReadOnlyList<LaboratoryOrderListItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        await SynchronizeCompletedSalesAsync(cancellationToken);
        return (await repository.ListAsync(cancellationToken)).Select(ToListItem).ToArray();
    }

    public async Task<LaboratoryOrderListItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToListItem(order);
    }

    public async Task UpdateAsync(Guid id, OpticalLaboratory laboratory, LaboratoryOrderStatus status, DateOnly? expectedDate, string? notes, string? documentFileName = null, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Pedido de laboratório não encontrado.");
        if (order.Laboratory != laboratory) order.ChangeLaboratory(laboratory);
        order.Update(status, expectedDate, notes);
        if (documentFileName is not null) order.SetDocument(documentFileName);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateForSaleAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        foreach (var item in sale.Items.Where(item => item.IncludesLenses))
        {
            if (!await repository.ExistsForSaleItemAsync(item.Id, cancellationToken))
                await repository.AddAsync(new LaboratoryOrder(sale, item), cancellationToken);
        }
    }

    private async Task SynchronizeCompletedSalesAsync(CancellationToken cancellationToken)
    {
        var added = false;
        foreach (var sale in (await saleRepository.ListAsync(cancellationToken)).Where(sale => sale.Status == SaleStatus.Completed))
        {
            foreach (var item in sale.Items.Where(item => item.IncludesLenses))
            {
                if (await repository.ExistsForSaleItemAsync(item.Id, cancellationToken)) continue;
                await repository.AddAsync(new LaboratoryOrder(sale, item), cancellationToken);
                added = true;
            }
        }
        if (added) await repository.SaveChangesAsync(cancellationToken);
    }

    private static LaboratoryOrderListItem ToListItem(LaboratoryOrder order) => new(
        order.Id, order.SaleId, order.Sale.Customer.Name, order.Sale.Customer.Phone,
        $"{order.SaleItem.FrameBrand} {order.SaleItem.FrameModel} · {order.SaleItem.FrameCode}",
        order.SaleItem.LensDescription ?? "Lentes", order.SaleItem.Quantity,
        order.Laboratory, order.Status, order.ExpectedDeliveryDate, order.Notes, order.SaleItem.Prescription, order.DocumentFileName,
        order.SentAtUtc, order.ReadyAtUtc, order.DeliveredAtUtc, order.CreatedAtUtc, order.UpdatedAtUtc,
        order.History.OrderByDescending(entry => entry.ChangedAtUtc)
            .Select(entry => new LaboratoryOrderHistoryItem(entry.Status, entry.ChangedAtUtc)).ToArray());
}
