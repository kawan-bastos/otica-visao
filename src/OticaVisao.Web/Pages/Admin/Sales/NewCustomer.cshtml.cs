using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Customers;
using OticaVisao.Application.Sales;
using OticaVisao.Infrastructure.Persistence;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Sales;

public sealed class NewCustomerModel(
    CustomerService customerService,
    SaleService saleService,
    ApplicationDbContext context) : PageModel
{
    [BindProperty]
    public CustomerInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var customerId = await customerService.CreateAsync(Input.ToCreateRequest(), cancellationToken);
            var saleId = await saleService.CreateDraftAsync(customerId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RedirectToPage("Details", new { id = saleId });
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
