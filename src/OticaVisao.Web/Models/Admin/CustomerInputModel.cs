using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Customers;
using OticaVisao.Domain.Customers;

namespace OticaVisao.Web.Models.Admin;

public sealed class CustomerInputModel : IValidatableObject
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

    [Required(ErrorMessage = "Informe o CPF do cliente.")]
    [Display(Name = "CPF")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateOnly? BirthDate { get; set; }

    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(160, ErrorMessage = "O e-mail deve ter no máximo 160 caracteres.")]
    [Display(Name = "E-mail (opcional)")]
    public string? Email { get; set; }

    [StringLength(1000, ErrorMessage = "As observações devem ter no máximo 1000 caracteres.")]
    [Display(Name = "Observações (opcional)")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Informe o CEP.")]
    [StringLength(9, ErrorMessage = "O CEP deve ter no máximo 9 caracteres.")]
    [Display(Name = "CEP")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a rua.")]
    [StringLength(160, ErrorMessage = "A rua deve ter no máximo 160 caracteres.")]
    [Display(Name = "Rua")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(20, ErrorMessage = "O número deve ter no máximo 20 caracteres.")]
    [Display(Name = "Número")]
    public string Number { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "O complemento deve ter no máximo 80 caracteres.")]
    [Display(Name = "Complemento (opcional)")]
    public string? Complement { get; set; }

    [Required(ErrorMessage = "Informe o bairro.")]
    [StringLength(100, ErrorMessage = "O bairro deve ter no máximo 100 caracteres.")]
    [Display(Name = "Bairro")]
    public string Neighborhood { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade.")]
    [StringLength(100, ErrorMessage = "A cidade deve ter no máximo 100 caracteres.")]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o estado.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Informe a UF com duas letras.")]
    [Display(Name = "Estado (UF)")]
    public string State { get; set; } = "RJ";

    public CreateCustomerRequest ToCreateRequest() => new(
        Name, Phone, Cpf, BirthDate!.Value, PostalCode, Street, Number, Complement,
        Neighborhood, City, State, Email, Notes);
    public UpdateCustomerRequest ToUpdateRequest() => new(
        Name, Phone, Cpf, BirthDate!.Value, PostalCode, Street, Number, Complement,
        Neighborhood, City, State, Email, Notes);

    public static CustomerInputModel FromCustomer(CustomerListItem customer) => new()
    {
        Name = customer.Name,
        Phone = customer.Phone,
        Cpf = customer.Cpf ?? string.Empty,
        BirthDate = customer.BirthDate,
        PostalCode = customer.PostalCode ?? string.Empty,
        Street = customer.Street ?? string.Empty,
        Number = customer.Number ?? string.Empty,
        Complement = customer.Complement,
        Neighborhood = customer.Neighborhood ?? string.Empty,
        City = customer.City ?? string.Empty,
        State = customer.State ?? "RJ",
        Email = customer.Email,
        Notes = customer.Notes
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BrazilianCpf.IsValid(Cpf))
            yield return new ValidationResult("Informe um CPF válido.", [nameof(Cpf)]);

        if (BirthDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (BirthDate.Value > today || BirthDate.Value < today.AddYears(-120))
                yield return new ValidationResult("Informe uma data de nascimento válida.", [nameof(BirthDate)]);
        }
    }
}
