using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.LaboratoryOrders;

namespace OticaVisao.Web.Pages.Admin.LaboratoryOrders;

public sealed class IndexModel(LaboratoryOrderService service) : PageModel
{
    public IReadOnlyList<LaboratoryOrderListItem> Orders { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken cancellationToken) => Orders = await service.ListAsync(cancellationToken);
}
