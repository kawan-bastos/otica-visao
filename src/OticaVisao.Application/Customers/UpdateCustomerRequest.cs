namespace OticaVisao.Application.Customers;

public sealed record UpdateCustomerRequest(
    string Name, string Phone, string Cpf, DateOnly BirthDate,
    string PostalCode, string Street, string Number, string? Complement,
    string Neighborhood, string City, string State, string? Email, string? Notes);
