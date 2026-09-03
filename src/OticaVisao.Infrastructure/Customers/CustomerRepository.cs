using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Customers;
using OticaVisao.Domain.Customers;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Customers;

public sealed class CustomerRepository(ApplicationDbContext context) : ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> ListAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = context.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(customer =>
                EF.Functions.ILike(customer.Name, pattern)
                || EF.Functions.ILike(customer.Phone, pattern)
                || (customer.Cpf != null && EF.Functions.ILike(customer.Cpf, pattern))
                || (customer.Email != null && EF.Functions.ILike(customer.Email, pattern)));
        }

        return await query.OrderBy(customer => customer.Name).ToListAsync(cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default) =>
        context.Customers.SingleOrDefaultAsync(customer => customer.Cpf == cpf, cancellationToken);

    public Task<bool> CpfExistsAsync(string cpf, Guid? excludingId = null, CancellationToken cancellationToken = default) =>
        context.Customers.AnyAsync(customer => customer.Cpf == cpf && (!excludingId.HasValue || customer.Id != excludingId), cancellationToken);

    public Task<bool> HasSalesAsync(Guid customerId, CancellationToken cancellationToken = default) =>
        context.Sales.AnyAsync(sale => sale.CustomerId == customerId, cancellationToken);

    public Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        context.Customers.AddAsync(customer, cancellationToken).AsTask();

    public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        if (customer.AccountUserId.HasValue)
        {
            var account = await context.Users.SingleOrDefaultAsync(user => user.Id == customer.AccountUserId.Value, cancellationToken);
            if (account is not null) context.Users.Remove(account);
        }
        context.Customers.Remove(customer);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
