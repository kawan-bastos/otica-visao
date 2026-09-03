using System.ComponentModel.DataAnnotations;

namespace OticaVisao.Web.Models.Admin;

public sealed class ReverseSaleInputModel
{
    [Required(ErrorMessage = "Informe o motivo do estorno.")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "O motivo deve possuir entre 3 e 500 caracteres.")]
    [Display(Name = "Motivo do estorno")]
    public string Reason { get; set; } = string.Empty;
}
