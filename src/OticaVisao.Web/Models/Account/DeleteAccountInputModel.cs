using System.ComponentModel.DataAnnotations;

namespace OticaVisao.Web.Models.Account;

public sealed class DeleteAccountInputModel
{
    [Required(ErrorMessage = "Informe sua senha atual para confirmar.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string Password { get; set; } = string.Empty;
}
