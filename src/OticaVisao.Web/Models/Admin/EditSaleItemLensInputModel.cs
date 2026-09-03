using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class EditSaleItemLensInputModel
{
    [Required(ErrorMessage = "Descreva as lentes.")]
    [StringLength(500)]
    [Display(Name = "Descrição das lentes")]
    public string? LensDescription { get; set; }
    [Range(typeof(decimal), "0", "99999999")]
    [Display(Name = "Valor das lentes")]
    public decimal LensUnitPrice { get; set; }
    [Required(ErrorMessage = "Selecione o laboratório.")]
    [Display(Name = "Laboratório")]
    public OpticalLaboratory? Laboratory { get; set; }
    public PrescriptionInputModel Prescription { get; set; } = new();
    [Display(Name = "Previsão de entrega")]
    [DataType(DataType.Date)]
    public DateOnly? ExpectedDeliveryDate { get; set; }

    public UpdateSaleItemLensRequest ToRequest() => new(LensDescription!, LensUnitPrice, Laboratory!.Value, Prescription.ToDomain(), ExpectedDeliveryDate);
}
