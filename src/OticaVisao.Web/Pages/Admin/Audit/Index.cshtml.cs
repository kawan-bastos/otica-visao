using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using OticaVisao.Application.Auditing;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Web.Pages.Admin.Audit;

public sealed class IndexModel(
    AuditLogService service,
    UserManager<ApplicationUser> userManager) : PageModel
{
    public IReadOnlyList<AuditLogListItem> Logs { get; private set; } = [];
    public IReadOnlyDictionary<string, string> ResponsibleNames { get; private set; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Logs = await service.ListRecentAsync(cancellationToken);
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var email in Logs.Select(log => log.PerformedBy)
                     .Where(value => value.Contains('@'))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var user = await userManager.FindByEmailAsync(email);
            if (!string.IsNullOrWhiteSpace(user?.DisplayName)) names[email] = user.DisplayName;
        }
        ResponsibleNames = names;
    }

    public string ResponsibleName(string storedValue) =>
        ResponsibleNames.TryGetValue(storedValue, out var name) ? name : storedValue;
}
