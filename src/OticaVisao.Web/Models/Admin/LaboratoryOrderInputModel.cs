using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class LaboratoryOrderInputModel
{
    [Required(ErrorMessage = "Selecione o laboratório.")]
    [Display(Name = "Laboratório")]
    public OpticalLaboratory? Laboratory { get; set; }
    [Required(ErrorMessage = "Selecione a situação do pedido.")]
    [Display(Name = "Situação")]
    public LaboratoryOrderStatus Status { get; set; }

    [Display(Name = "Previsão de entrega")]
    [DataType(DataType.Date)]
    public DateOnly? ExpectedDeliveryDate { get; set; }

    [StringLength(1000, ErrorMessage = "As observações devem possuir no máximo 1000 caracteres.")]
    [Display(Name = "Observações")]
    public string? Notes { get; set; }
}
