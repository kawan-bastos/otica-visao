using System.ComponentModel.DataAnnotations;

namespace OticaVisao.Web.Models.Account;

public sealed class CustomerRegisterInputModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Informe um telefone válido.")]
    [StringLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres.")]
    [Display(Name = "Telefone (opcional)")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Informe uma senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a senha.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "As senhas não são iguais.")]
    [Display(Name = "Confirmar senha")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
