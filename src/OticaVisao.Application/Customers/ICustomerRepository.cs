using OticaVisao.Domain.Customers;

namespace OticaVisao.Application.Customers;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task<bool> CpfExistsAsync(string cpf, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
