using OticaVisao.Domain.Customers;

namespace OticaVisao.Application.Customers;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
