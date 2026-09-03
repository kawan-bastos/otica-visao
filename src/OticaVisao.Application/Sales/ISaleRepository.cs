using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Sales;

public interface ISaleRepository
{
    Task<IReadOnlyList<Sale>> ListAsync(CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    void Remove(Sale sale);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
