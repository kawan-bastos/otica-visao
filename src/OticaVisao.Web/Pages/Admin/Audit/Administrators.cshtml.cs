using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OticaVisao.Application.Auditing;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Admin.Audit;

[Authorize(Policy = AdminAuthorization.GeneralPolicy)]
public sealed class AdministratorsModel(
    UserManager<ApplicationUser> userManager,
    AuditLogService auditLogService) : PageModel
{
    [BindProperty]
    public NewAdministratorInput Input { get; set; } = new();

    public IReadOnlyList<AdministratorItem> Administrators { get; private set; } = [];

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var email = Input.Email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError(nameof(Input.Email), "Já existe uma conta com este e-mail.");
            await LoadAsync();
            return Page();
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = Input.DisplayName.Trim()
        };
        var creation = await userManager.CreateAsync(user, Input.Password);
        if (!creation.Succeeded)
        {
            AddErrors(creation);
            await LoadAsync();
            return Page();
        }

        var authorization = await userManager.AddToRoleAsync(user, AdminAuthorization.Role);
        if (!authorization.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddErrors(authorization);
            await LoadAsync();
            return Page();
        }

        if (!Input.IsGeneral && !Input.AllPanels && Input.Permissions.Count == 0)
        {
            await userManager.DeleteAsync(user);
            ModelState.AddModelError(nameof(Input.Permissions), "Selecione pelo menos um painel ou libere todos os painéis.");
            await LoadAsync();
            return Page();
        }

        await SetPermissionsAsync(user, Input.AllPanels, Input.Permissions);
        if (Input.IsGeneral) AddErrors(await userManager.AddToRoleAsync(user, AdminAuthorization.GeneralRole));

        await auditLogService.RegisterAsync("Administrador cadastrado", "Administrador", user.Id.ToString(),
            $"{user.DisplayName} — {email}. Acesso: {DescribeAccess(Input.IsGeneral, Input.AllPanels, Input.Permissions)}",
            CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = "Administrador cadastrado com sucesso.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || !await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return NotFound();
        if (string.Equals(userManager.GetUserId(User), user.Id.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            TempData["AdminError"] = "Você não pode remover a própria conta.";
            return RedirectToPage();
        }

        var displayName = user.DisplayName;
        var email = user.Email ?? string.Empty;
        var deletion = await userManager.DeleteAsync(user);
        if (!deletion.Succeeded)
        {
            TempData["AdminError"] = string.Join(" ", deletion.Errors.Select(error => error.Description));
            return RedirectToPage();
        }

        await auditLogService.RegisterAsync("Administrador removido", "Administrador", user.Id.ToString(),
            $"{displayName} — {email}", CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = "Administrador removido.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRoleAsync(Guid id, string role, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null || !await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return NotFound();
        var makeGeneral = string.Equals(role, "general", StringComparison.OrdinalIgnoreCase);
        var isOwnAccount = string.Equals(userManager.GetUserId(User), user.Id.ToString(), StringComparison.OrdinalIgnoreCase);
        if (isOwnAccount && !makeGeneral)
        {
            TempData["AdminError"] = "Você não pode retirar o próprio acesso de administrador geral.";
            return RedirectToPage();
        }

        var currentlyGeneral = await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole);
        var result = makeGeneral && !currentlyGeneral
            ? await userManager.AddToRoleAsync(user, AdminAuthorization.GeneralRole)
            : !makeGeneral && currentlyGeneral
                ? await userManager.RemoveFromRoleAsync(user, AdminAuthorization.GeneralRole)
                : IdentityResult.Success;
        if (!result.Succeeded)
        {
            TempData["AdminError"] = string.Join(" ", result.Errors.Select(error => error.Description));
            return RedirectToPage();
        }

        await auditLogService.RegisterAsync("Função administrativa alterada", "Administrador", user.Id.ToString(),
            $"{user.DisplayName} — {(makeGeneral ? "Administrador geral" : "Administrador")}", CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = "Função atualizada com sucesso.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkGrantAllAsync(List<Guid> selectedIds, CancellationToken cancellationToken)
    {
        var changed = 0;
        foreach (var id in selectedIds.Distinct())
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null || !await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) continue;
            await SetPermissionsAsync(user, true, []);
            changed++;
        }
        if (changed > 0)
            await auditLogService.RegisterAsync("Acesso total liberado em massa", "Administradores", "multiple",
                $"{changed} conta(s) atualizada(s)", CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = $"Todos os painéis foram liberados para {changed} conta(s).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(List<Guid> selectedIds, CancellationToken cancellationToken)
    {
        var currentId = userManager.GetUserId(User);
        var removed = 0;
        foreach (var id in selectedIds.Distinct())
        {
            if (string.Equals(currentId, id.ToString(), StringComparison.OrdinalIgnoreCase)) continue;
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user is null || !await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) continue;
            if ((await userManager.DeleteAsync(user)).Succeeded) removed++;
        }
        if (removed > 0)
            await auditLogService.RegisterAsync("Administradores removidos em massa", "Administradores", "multiple",
                $"{removed} conta(s) removida(s)", CurrentActor(), cancellationToken);
        TempData["AdminMessage"] = $"{removed} conta(s) removida(s). Sua própria conta foi preservada.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        var users = await userManager.GetUsersInRoleAsync(AdminAuthorization.Role);
        var items = new List<AdministratorItem>(users.Count);
        foreach (var user in users.OrderBy(user => user.DisplayName))
        {
            var claims = await userManager.GetClaimsAsync(user);
            var permissions = claims.Where(claim => claim.Type == AdminAuthorization.PermissionClaim)
                .Select(claim => claim.Value).ToHashSet(StringComparer.Ordinal);
            items.Add(new(user.Id, user.DisplayName, user.Email ?? string.Empty,
                await userManager.IsInRoleAsync(user, AdminAuthorization.GeneralRole),
                permissions.Contains(AdminAuthorization.AllPanelsPermission), permissions));
        }
        Administrators = items;
    }

    private async Task SetPermissionsAsync(ApplicationUser user, bool allPanels, IEnumerable<string> selected)
    {
        var existing = (await userManager.GetClaimsAsync(user))
            .Where(claim => claim.Type == AdminAuthorization.PermissionClaim).ToArray();
        if (existing.Length > 0) AddErrors(await userManager.RemoveClaimsAsync(user, existing));

        var allowed = AdminAuthorization.Permissions.Select(permission => permission.Value).ToHashSet(StringComparer.Ordinal);
        var values = allPanels
            ? [AdminAuthorization.AllPanelsPermission]
            : selected.Where(allowed.Contains).Distinct(StringComparer.Ordinal).ToArray();
        if (values.Length > 0)
            AddErrors(await userManager.AddClaimsAsync(user,
                values.Select(value => new Claim(AdminAuthorization.PermissionClaim, value))));
    }

    private static string DescribeAccess(bool isGeneral, bool allPanels, IEnumerable<string> selected)
    {
        if (isGeneral) return "administrador geral (todos os painéis)";
        if (allPanels) return "todos os painéis";
        var values = selected.ToHashSet(StringComparer.Ordinal);
        return string.Join(", ", AdminAuthorization.Permissions
            .Where(permission => values.Contains(permission.Value)).Select(permission => permission.Label));
    }

    private string CurrentActor() => User.FindFirst(AdminAuthorization.DisplayNameClaim)?.Value
        ?? User.Identity?.Name ?? "Administrador geral";

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

    public sealed class NewAdministratorInput
    {
        [Required(ErrorMessage = "Informe o nome."), StringLength(100)]
        [Display(Name = "Nome")]
        public string DisplayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha."), DataType(DataType.Password), MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
        [Display(Name = "Senha temporária")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha."), DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "As senhas não coincidem.")]
        [Display(Name = "Confirmar senha")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsGeneral { get; set; }

        public bool AllPanels { get; set; }

        public List<string> Permissions { get; set; } = [];
    }

    public sealed record AdministratorItem(Guid Id, string DisplayName, string Email, bool IsGeneral,
        bool AllPanels, IReadOnlySet<string> Permissions);
}
