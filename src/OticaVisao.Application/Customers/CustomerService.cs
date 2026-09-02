using OticaVisao.Domain.Customers;

namespace OticaVisao.Application.Customers;

public sealed class CustomerService(ICustomerRepository repository)
{
    public async Task<IReadOnlyList<CustomerListItem>> ListAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var customers = await repository.ListAsync(search, cancellationToken);
        return customers.Select(ToListItem).ToArray();
    }

    public async Task<CustomerListItem?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : ToListItem(customer);
    }

    public async Task<Guid> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var customer = new Customer(request.Name, request.Phone, request.Email, request.Notes);
        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var customer = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        customer.Update(request.Name, request.Phone, request.Email, request.Notes);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static CustomerListItem ToListItem(Customer customer) => new(
        customer.Id, customer.Name, customer.Phone, customer.Email, customer.Notes,
        customer.CreatedAtUtc, customer.UpdatedAtUtc);
}
