using System.ComponentModel.DataAnnotations;

namespace OticaVisao.Web.Models.Account;

public sealed class ResetPasswordInputModel
{
    [Required(ErrorMessage = "Informe o código recebido.")]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "O código deve conter 6 números.")]
    [Display(Name = "Código de verificação")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "As senhas não coincidem.")]
    [Display(Name = "Confirmar nova senha")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
