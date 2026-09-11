using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Web.Pages.Account;

[AllowAnonymous]
public sealed class ResetPasswordModel(
    UserManager<ApplicationUser> userManager,
    IDistributedCache cache,
    PasswordHistoryService passwordHistory) : PageModel
{
    private const int MaximumAttempts = 5;

    [BindProperty(SupportsGet = true)]
    public string RequestId { get; set; } = string.Empty;

    [BindProperty]
    public ResetPasswordInputModel Input { get; set; } = new();

    public IActionResult OnGet() => IsValidRequestId(RequestId) ? Page() : RedirectToPage("ForgotPassword");

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();
        if (!IsValidRequestId(RequestId)) return InvalidCode();

        var key = ForgotPasswordModel.CacheKey(RequestId);
        var serialized = await cache.GetStringAsync(key, cancellationToken);
        var request = serialized is null
            ? null
            : JsonSerializer.Deserialize<ForgotPasswordModel.PasswordResetRequest>(serialized);

        if (request is null || request.ExpiresAtUtc <= DateTimeOffset.UtcNow
            || request.FailedAttempts >= MaximumAttempts)
        {
            await cache.RemoveAsync(key, cancellationToken);
            return InvalidCode();
        }

        var expectedHash = Convert.FromHexString(request.CodeHash);
        var suppliedHash = Convert.FromHexString(ForgotPasswordModel.HashCode(RequestId, Input.Code));
        if (!CryptographicOperations.FixedTimeEquals(expectedHash, suppliedHash))
        {
            request = request with { FailedAttempts = request.FailedAttempts + 1 };
            await cache.SetStringAsync(key, JsonSerializer.Serialize(request),
                new DistributedCacheEntryOptions { AbsoluteExpiration = request.ExpiresAtUtc }, cancellationToken);
            ModelState.AddModelError("Input.Code", request.FailedAttempts >= MaximumAttempts
                ? "Código bloqueado após muitas tentativas. Solicite um novo."
                : "Código incorreto ou expirado.");
            return Page();
        }

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || await userManager.IsInRoleAsync(user, AdminAuthorization.Role)) return InvalidCode();

        if (await passwordHistory.WasPreviouslyUsedAsync(user, Input.NewPassword, cancellationToken))
        {
            ModelState.AddModelError("Input.NewPassword", "Escolha uma senha que não tenha sido usada anteriormente.");
            return Page();
        }

        var previousPasswordHash = user.PasswordHash;
        var result = await userManager.ResetPasswordAsync(user, request.ResetToken, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Code.StartsWith("Password", StringComparison.Ordinal)
                    ? "A nova senha não atende aos requisitos de segurança."
                    : "Não foi possível redefinir a senha. Solicite um novo código.");
            return Page();
        }

        await passwordHistory.RecordPreviousPasswordAsync(user, previousPasswordHash, cancellationToken);
        await cache.RemoveAsync(key, cancellationToken);
        TempData["LoginSuccessMessage"] = "Senha redefinida. Você já pode entrar com a nova senha.";
        return RedirectToPage("Login");
    }

    private PageResult InvalidCode()
    {
        ModelState.AddModelError(string.Empty, "Código incorreto ou expirado. Solicite um novo código.");
        return Page();
    }

    private static bool IsValidRequestId(string value) =>
        value.Length == 32 && Guid.TryParseExact(value, "N", out _);
}
