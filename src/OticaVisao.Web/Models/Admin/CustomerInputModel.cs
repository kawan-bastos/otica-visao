using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Customers;

namespace OticaVisao.Web.Models.Admin;

public sealed class CustomerInputModel
{
    [Required(ErrorMessage = "Informe o nome do cliente.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    [Display(Name = "Nome completo")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone do cliente.")]
    [Phone(ErrorMessage = "Informe um telefone válido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [Display(Name = "Telefone")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(160, ErrorMessage = "O e-mail deve ter no máximo 160 caracteres.")]
    [Display(Name = "E-mail (opcional)")]
    public string? Email { get; set; }

    [StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres.")]
    [Display(Name = "Observações (opcional)")]
    public string? Notes { get; set; }

    public CreateCustomerRequest ToCreateRequest() => new(Name, Phone, Email, Notes);
    public UpdateCustomerRequest ToUpdateRequest() => new(Name, Phone, Email, Notes);

    public static CustomerInputModel FromCustomer(CustomerListItem customer) => new()
    {
        Name = customer.Name,
        Phone = customer.Phone,
        Email = customer.Email,
        Notes = customer.Notes
    };
}
