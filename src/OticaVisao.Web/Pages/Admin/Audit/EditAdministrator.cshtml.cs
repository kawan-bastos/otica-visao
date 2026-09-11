using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Auditing;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Admin.Audit;

[Authorize(Policy = AdminAuthorization.GeneralPolicy)]
public sealed class EditAdministratorModel(
    UserManager<ApplicationUser> userManager,
    AuditLogService auditLogService,
    PasswordHistoryService passwordHistory) : PageModel
{
    [BindProperty]
    public EditAdministratorInput Input { get; set; } = new();

    public Guid AdministratorId { get; private set; }
    public bool IsOwnAccount { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var user = await FindAdministratorAsync(id);
        if (user is null) return NotFound();
        await PopulateAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await FindAdministratorAsync(id);
        if (user is null) return NotFound();
        AdministratorId = user.Id;
        IsOwnAccount = string.Equals(userManager.GetUserId(User), user.Id.ToString(), StringComparison.OrdinalIgnoreCase);

        var selected = Input.Permissions.Where(value => AdminAuthorization.Permissions.Any(option => option.Value == value))
            .Distinct(StringComparer.Ordinal).ToArray();
        if (!Input.IsGeneral && !Input.AllPanels && selected.Length == 0)
            ModelState.AddModelError(nameof(Input.Permissions), "Selecione pelo menos um painel ou libere todos os painéis.");
        if (IsOwnAccount && !Input.IsGeneral)
            ModelState.AddModelError(nameof(Input.IsGeneral), "Você não pode retirar o próprio acesso de administrador geral.");

        var email = Input.Email.Trim().ToLowerInvariant();
        var existingEmail = await userManager.FindByEmailAsync(email);
        if (existingEmail is not null && existingEmail.Id != user.Id)
            ModelState.AddModelError(nameof(Input.Email), "Já existe uma conta com este e-mail.");

        if (!ModelState.IsValid) return Page();

        user.DisplayName = Input.DisplayName.Trim();
        user.Email = email;
        user.UserName = email;
        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            AddErrors(update);
            return Page();
        }

        var currentlyGeneral = await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole);
        if (Input.IsGeneral && !currentlyGeneral) AddErrors(await userManager.AddToRoleAsync(user, AdminAuthorization.GeneralRole));
        if (!Input.IsGeneral && currentlyGeneral) AddErrors(await userManager.RemoveFromRoleAsync(user, AdminAuthorization.GeneralRole));
        await SetPermissionsAsync(user, Input.AllPanels, selected);

        if (!string.IsNullOrWhiteSpace(Input.NewPassword))
        {
            if (await passwordHistory.WasPreviouslyUsedAsync(user, Input.NewPassword, cancellationToken))
            {
                ModelState.AddModelError(nameof(Input.NewPassword), "Escolha uma senha que não tenha sido usada anteriormente.");
                return Page();
            }

            var previousPasswordHash = user.PasswordHash;
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var password = await userManager.ResetPasswordAsync(user, token, Input.NewPassword);
            if (!password.Succeeded)
            {
                AddErrors(password);
                return Page();
            }
            await passwordHistory.RecordPreviousPasswordAsync(user, previousPasswordHash, cancellationToken);
        }

        await auditLogService.RegisterAsync("Administrador atualizado", "Administrador", user.Id.ToString(),
            $"{user.DisplayName} — {email}. Acesso: {DescribeAccess(Input.IsGeneral, Input.AllPanels, selected)}" +
            (string.IsNullOrWhiteSpace(Input.NewPassword) ? string.Empty : ". Senha redefinida"),
            CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = "Administrador e permissões atualizados com sucesso.";
        return RedirectToPage("Administrators");
    }

    private async Task<ApplicationUser?> FindAdministratorAsync(Guid id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        return user is not null && await userManager.IsInRoleAsync(user, AdminAuthorization.Role) ? user : null;
    }

    private async Task PopulateAsync(ApplicationUser user)
    {
        AdministratorId = user.Id;
        IsOwnAccount = string.Equals(userManager.GetUserId(User), user.Id.ToString(), StringComparison.OrdinalIgnoreCase);
        var claims = await userManager.GetClaimsAsync(user);
        var permissions = claims.Where(claim => claim.Type == AdminAuthorization.PermissionClaim)
            .Select(claim => claim.Value).ToList();
        Input = new EditAdministratorInput
        {
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            IsGeneral = await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole),
            AllPanels = permissions.Contains(AdminAuthorization.AllPanelsPermission),
            Permissions = permissions.Where(value => value != AdminAuthorization.AllPanelsPermission).ToList()
        };
    }

    private async Task SetPermissionsAsync(ApplicationUser user, bool allPanels, IEnumerable<string> selected)
    {
        var existing = (await userManager.GetClaimsAsync(user))
            .Where(claim => claim.Type == AdminAuthorization.PermissionClaim).ToArray();
        if (existing.Length > 0) AddErrors(await userManager.RemoveClaimsAsync(user, existing));
        var values = allPanels ? [AdminAuthorization.AllPanelsPermission] : selected.ToArray();
        if (values.Length > 0)
            AddErrors(await userManager.AddClaimsAsync(user,
                values.Select(value => new Claim(AdminAuthorization.PermissionClaim, value))));
    }

    private static string DescribeAccess(bool general, bool allPanels, IEnumerable<string> selected)
    {
        if (general) return "administrador geral (todos os painéis)";
        if (allPanels) return "todos os painéis";
        var values = selected.ToHashSet(StringComparer.Ordinal);
        return string.Join(", ", AdminAuthorization.Permissions.Where(item => values.Contains(item.Value)).Select(item => item.Label));
    }

    private string CurrentActor() => User.FindFirst(AdminAuthorization.DisplayNameClaim)?.Value
        ?? User.Identity?.Name ?? "Administrador geral";

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

    public sealed class EditAdministratorInput
    {
        [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
        [Display(Name = "Nome")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password), MinLength(8, ErrorMessage = "A nova senha deve ter pelo menos 8 caracteres.")]
        [Display(Name = "Nova senha")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "As senhas não coincidem.")]
        [Display(Name = "Confirmar nova senha")]
        public string? ConfirmPassword { get; set; }

        public bool IsGeneral { get; set; }
        public bool AllPanels { get; set; }
        public List<string> Permissions { get; set; } = [];
    }
}
