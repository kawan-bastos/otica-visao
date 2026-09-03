using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class SaleItemInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecione uma armação.")]
    [Display(Name = "Armação")]
    public Guid? FrameId { get; set; }

    [Range(1, 100, ErrorMessage = "A quantidade deve estar entre 1 e 100.")]
    [Display(Name = "Quantidade")]
    public int Quantity { get; set; } = 1;

    [Display(Name = "Óculos completo com lentes")]
    public bool IncludesLenses { get; set; } = true;

    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    [Display(Name = "Descrição das lentes")]
    public string? LensDescription { get; set; }

    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "Informe um valor válido para as lentes.")]
    [Display(Name = "Valor das lentes")]
    public decimal LensUnitPrice { get; set; }

    [Display(Name = "Laboratório")]
    public OpticalLaboratory? Laboratory { get; set; }

    public AddSaleItemRequest ToRequest() => new(
        FrameId!.Value, Quantity, IncludesLenses, IncludesLenses ? LensDescription : null,
        IncludesLenses ? LensUnitPrice : 0, IncludesLenses ? Laboratory : null);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IncludesLenses) yield break;
        if (string.IsNullOrWhiteSpace(LensDescription))
            yield return new ValidationResult("Descreva as lentes.", [nameof(LensDescription)]);
        if (!Laboratory.HasValue)
            yield return new ValidationResult("Selecione o laboratório.", [nameof(Laboratory)]);
    }
}
