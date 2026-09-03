using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;

namespace OticaVisao.Web.Pages.Admin.Customers;

public sealed class DeleteModel(CustomerService customerService) : PageModel
{
    public CustomerListItem Customer { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        Customer = customer;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await customerService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = "Cliente excluído permanentemente.";
            return RedirectToPage("Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException exception)
        {
            var customer = await customerService.GetAsync(id, cancellationToken);
            if (customer is null) return NotFound();
            Customer = customer;
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
    }
}
