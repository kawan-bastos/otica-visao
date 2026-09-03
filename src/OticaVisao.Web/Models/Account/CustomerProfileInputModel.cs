using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.Customers;

namespace OticaVisao.Web.Models.Account;

public class CustomerProfileInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu telefone.")]
    [Phone(ErrorMessage = "Informe um telefone válido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [Display(Name = "Telefone/WhatsApp")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu CPF.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua data de nascimento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de nascimento")]
    public DateOnly? BirthDate { get; set; }

    [Required(ErrorMessage = "Informe seu CEP.")]
    [StringLength(9)]
    [Display(Name = "CEP")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua rua.")]
    [StringLength(160)]
    [Display(Name = "Rua")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(20)]
    [Display(Name = "Número")]
    public string Number { get; set; } = string.Empty;

    [StringLength(80)]
    [Display(Name = "Complemento (opcional)")]
    public string? Complement { get; set; }

    [Required(ErrorMessage = "Informe seu bairro.")]
    [StringLength(100)]
    [Display(Name = "Bairro")]
    public string Neighborhood { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua cidade.")]
    [StringLength(100)]
    [Display(Name = "Cidade")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu estado.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Informe a UF com duas letras.")]
    [Display(Name = "Estado (UF)")]
    public string State { get; set; } = "RJ";

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
