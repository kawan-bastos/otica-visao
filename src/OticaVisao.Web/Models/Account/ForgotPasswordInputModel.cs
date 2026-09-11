using System.ComponentModel.DataAnnotations;

namespace OticaVisao.Web.Models.Account;

public sealed class ForgotPasswordInputModel
{
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail da conta")]
    public string Email { get; set; } = string.Empty;
}
