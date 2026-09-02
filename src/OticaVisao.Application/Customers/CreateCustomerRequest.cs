namespace OticaVisao.Application.Customers;

public sealed record CreateCustomerRequest(string Name, string Phone, string? Email, string? Notes);
