using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.LaboratoryOrders;

namespace OticaVisao.Web.Pages.Admin.LaboratoryOrders;

public sealed class IndexModel(LaboratoryOrderService service) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<LaboratoryOrderListItem> Orders { get; private set; } = [];
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var orders = await service.ListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            orders = orders.Where(order =>
                order.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                order.CustomerPhone.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                order.FrameDescription.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                order.LensDescription.Contains(term, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        Orders = orders;
    }
}
