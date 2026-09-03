using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Auditing;

namespace OticaVisao.Web.Pages.Admin.Audit;

public sealed class IndexModel(AuditLogService service) : PageModel
{
    public IReadOnlyList<AuditLogListItem> Logs { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Logs = await service.ListRecentAsync(cancellationToken);
}
