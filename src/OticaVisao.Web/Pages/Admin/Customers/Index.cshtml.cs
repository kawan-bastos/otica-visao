using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;

namespace OticaVisao.Web.Pages.Admin.Customers;

public sealed class IndexModel(CustomerService customerService, OticaVisao.Application.Sales.SaleService saleService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<CustomerListItem> Customers { get; private set; } = [];
    public IReadOnlyDictionary<Guid, int> SalesCountByCustomer { get; private set; } = new Dictionary<Guid, int>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Customers = await customerService.ListAsync(Search, cancellationToken);
        SalesCountByCustomer = (await saleService.ListAsync(cancellationToken))
            .Where(sale => sale.Status == OticaVisao.Domain.Sales.SaleStatus.Completed)
            .GroupBy(sale => sale.CustomerId).ToDictionary(group => group.Key, group => group.Count());
    }
}
