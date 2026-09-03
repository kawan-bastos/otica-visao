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
        var cpf = BrazilianCpf.Normalize(request.Cpf);
        if (await repository.CpfExistsAsync(cpf, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Já existe um cliente cadastrado com este CPF.");
        }

        var customer = new Customer(
            request.Name, request.Phone, cpf, request.BirthDate, CreateAddress(request), request.Email, request.Notes);
        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var customer = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        var cpf = BrazilianCpf.Normalize(request.Cpf);
        if (await repository.CpfExistsAsync(cpf, id, cancellationToken))
        {
            throw new InvalidOperationException("Já existe outro cliente cadastrado com este CPF.");
        }

        customer.Update(
            request.Name, request.Phone, cpf, request.BirthDate, CreateAddress(request), request.Email, request.Notes);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        if (await repository.HasSalesAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("Este cliente possui vendas e não pode ser excluído. A ficha deve ser preservada para manter o histórico comercial.");
        }

        await repository.DeleteAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static CustomerListItem ToListItem(Customer customer) => new(
        customer.Id, customer.Name, customer.Phone, customer.Cpf, customer.BirthDate,
        customer.Address?.PostalCode, customer.Address?.Street, customer.Address?.Number,
        customer.Address?.Complement, customer.Address?.Neighborhood, customer.Address?.City, customer.Address?.State,
        customer.Email, customer.Notes, customer.AccountUserId,
        customer.CreatedAtUtc, customer.UpdatedAtUtc);

    private static CustomerAddress CreateAddress(CreateCustomerRequest request) => new(
        request.PostalCode, request.Street, request.Number, request.Complement,
        request.Neighborhood, request.City, request.State);

    private static CustomerAddress CreateAddress(UpdateCustomerRequest request) => new(
        request.PostalCode, request.Street, request.Number, request.Complement,
        request.Neighborhood, request.City, request.State);
}
