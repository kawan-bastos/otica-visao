using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;

namespace OticaVisao.Web.Pages.Admin.Customers;

public sealed class IndexModel(CustomerService customerService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public IReadOnlyList<CustomerListItem> Customers { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Customers = await customerService.ListAsync(Search, cancellationToken);
}
