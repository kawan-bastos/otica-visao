using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Authentication;

public sealed class PasswordHistoryService(
    ApplicationDbContext context,
    IPasswordHasher<ApplicationUser> passwordHasher)
{
    private const int RetainedPasswords = 5;

    public async Task<bool> WasPreviouslyUsedAsync(
        ApplicationUser user,
        string candidatePassword,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(user.PasswordHash)
            && Matches(user, user.PasswordHash, candidatePassword))
            return true;

        var previousHashes = await context.PasswordHistory
            .AsNoTracking()
            .Where(entry => entry.UserId == user.Id)
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Select(entry => entry.PasswordHash)
            .Take(RetainedPasswords)
            .ToListAsync(cancellationToken);

        return previousHashes.Any(hash => Matches(user, hash, candidatePassword));
    }

    public async Task RecordPreviousPasswordAsync(
        ApplicationUser user,
        string? previousPasswordHash,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(previousPasswordHash)) return;

        var entriesToRemove = await context.PasswordHistory
            .Where(entry => entry.UserId == user.Id)
            .OrderByDescending(entry => entry.ChangedAtUtc)
            .Skip(RetainedPasswords - 1)
            .ToListAsync(cancellationToken);

        if (entriesToRemove.Count > 0) context.PasswordHistory.RemoveRange(entriesToRemove);
        context.PasswordHistory.Add(new PasswordHistoryEntry(user.Id, previousPasswordHash, DateTimeOffset.UtcNow));
        await context.SaveChangesAsync(cancellationToken);
    }

    private bool Matches(ApplicationUser user, string hash, string password) =>
        passwordHasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed;
}
