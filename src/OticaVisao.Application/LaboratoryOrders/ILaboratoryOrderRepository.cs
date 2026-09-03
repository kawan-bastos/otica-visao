using OticaVisao.Domain.LaboratoryOrders;

namespace OticaVisao.Application.LaboratoryOrders;

public interface ILaboratoryOrderRepository
{
    Task<IReadOnlyList<LaboratoryOrder>> ListAsync(CancellationToken cancellationToken = default);
    Task<LaboratoryOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForSaleItemAsync(Guid saleItemId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LaboratoryOrder>> ListBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task AddAsync(LaboratoryOrder order, CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<LaboratoryOrder> orders);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
