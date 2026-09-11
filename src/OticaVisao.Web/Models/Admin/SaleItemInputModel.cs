using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class SaleItemInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecione uma armação.")]
    [Display(Name = "Armação")]
    public Guid? FrameId { get; set; }

    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "Informe um valor válido para a armação.")]
    [Display(Name = "Valor da armação")]
    public decimal? FrameUnitPrice { get; set; }

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
    public decimal? LensUnitPrice { get; set; }

    [Display(Name = "Laboratório")]
    public OpticalLaboratory? Laboratory { get; set; }

    [Display(Name = "Previsão de entrega")]
    [DataType(DataType.Date)]
    public DateOnly? ExpectedDeliveryDate { get; set; }

    public PrescriptionInputModel Prescription { get; set; } = new();

    public AddSaleItemRequest ToRequest() => new(
        FrameId!.Value, Quantity, IncludesLenses, IncludesLenses ? LensDescription : null,
        IncludesLenses ? LensUnitPrice ?? 0 : 0, IncludesLenses ? Laboratory : null,
        IncludesLenses ? Prescription.ToDomain() : null,
        IncludesLenses ? ExpectedDeliveryDate : null,
        FrameUnitPrice ?? 0);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!FrameUnitPrice.HasValue)
            yield return new ValidationResult("Informe o valor cobrado pela armação.", [nameof(FrameUnitPrice)]);

        if (!IncludesLenses) yield break;
        if (string.IsNullOrWhiteSpace(LensDescription))
            yield return new ValidationResult("Descreva as lentes.", [nameof(LensDescription)]);
        if (!Laboratory.HasValue)
            yield return new ValidationResult("Selecione o laboratório.", [nameof(Laboratory)]);
        if (!LensUnitPrice.HasValue)
            yield return new ValidationResult("Informe o valor das lentes.", [nameof(LensUnitPrice)]);
    }
}
