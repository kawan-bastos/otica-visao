using System.Globalization;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Web.Models.Account;
using OticaVisao.Web.Services;

namespace OticaVisao.Web.Pages.Account;

[AllowAnonymous]
public sealed class ForgotPasswordModel(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    IDistributedCache cache,
    IAccountEmailSender emailSender,
    ILogger<ForgotPasswordModel> logger) : PageModel
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly Action<ILogger, Exception?> LogEmailFailure = LoggerMessage.Define(
        LogLevel.Error,
        new EventId(1, nameof(LogEmailFailure)),
        "Não foi possível enviar o código de recuperação de senha.");

    [BindProperty]
    public ForgotPasswordInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();

        var requestId = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        var user = await userManager.FindByEmailAsync(Input.Email.Trim());
        var hasCustomerProfile = user is not null && await context.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.AccountUserId == user.Id, cancellationToken);
        if (user is null
            || !hasCustomerProfile
            || await userManager.IsInRoleAsync(user, AdminAuthorization.Role))
        {
            ModelState.AddModelError("Input.Email", "Não encontramos uma conta cadastrada com este e-mail.");
            return Page();
        }

        var code = RandomNumberGenerator.GetInt32(0, 1_000_000)
            .ToString("D6", CultureInfo.InvariantCulture);
        var expiresAtUtc = DateTimeOffset.UtcNow.Add(CodeLifetime);
        var request = new PasswordResetRequest(
            user.Id,
            HashCode(requestId, code),
            await userManager.GeneratePasswordResetTokenAsync(user),
            expiresAtUtc,
            0);

        await cache.SetStringAsync(CacheKey(requestId), JsonSerializer.Serialize(request),
            new DistributedCacheEntryOptions { AbsoluteExpiration = expiresAtUtc }, cancellationToken);

        try
        {
            await emailSender.SendPasswordResetCodeAsync(
                user.Email!, user.DisplayName, code, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidOperationException or SmtpException)
        {
            await cache.RemoveAsync(CacheKey(requestId), cancellationToken);
            LogEmailFailure(logger, exception);
            ModelState.AddModelError(string.Empty,
                "Não foi possível enviar o e-mail agora. Tente novamente mais tarde.");
            return Page();
        }

        return RedirectToPage("ResetPassword", new { requestId });
    }

    internal static string CacheKey(string requestId) => $"password-reset:{requestId}";

    internal static string HashCode(string requestId, string code) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes($"{requestId}:{code}")));

    internal sealed record PasswordResetRequest(
        Guid UserId,
        string CodeHash,
        string ResetToken,
        DateTimeOffset ExpiresAtUtc,
        int FailedAttempts);
}
