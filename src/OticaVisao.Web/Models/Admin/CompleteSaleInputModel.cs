using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class CompleteSaleInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecione a forma de pagamento.")]
    [Display(Name = "Forma de pagamento")]
    public PaymentMethod? PaymentMethod { get; set; }

    [Range(1, 10, ErrorMessage = "Selecione entre 1 e 10 parcelas.")]
    [Display(Name = "Parcelas")]
    public int Installments { get; set; } = 1;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PaymentMethod is not null and not OticaVisao.Domain.Sales.PaymentMethod.CreditCard && Installments != 1)
            yield return new ValidationResult("Pix e débito são pagos em uma única vez.", [nameof(Installments)]);
    }
}
