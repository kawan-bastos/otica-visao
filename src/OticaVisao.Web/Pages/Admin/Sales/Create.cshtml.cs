using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;
using OticaVisao.Application.Sales;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class CreateModel(CustomerService customerService, SaleService saleService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }
    public IReadOnlyList<CustomerListItem> Customers { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Customers = await customerService.ListAsync(Search, cancellationToken);

    public async Task<IActionResult> OnPostSelectAsync(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var saleId = await saleService.CreateDraftAsync(customerId, cancellationToken);
            return RedirectToPage("Details", new { id = saleId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
