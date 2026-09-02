using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Customers;

public sealed class EditModel(CustomerService customerService) : PageModel
{
    [BindProperty] public CustomerInputModel Input { get; set; } = new();
    public Guid CustomerId { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetAsync(id, cancellationToken);
        if (customer is null) return NotFound();
        CustomerId = id;
        Input = CustomerInputModel.FromCustomer(customer);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        CustomerId = id;
        if (!ModelState.IsValid) return Page();
        try
        {
            await customerService.UpdateAsync(id, Input.ToUpdateRequest(), cancellationToken);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
        TempData["SuccessMessage"] = "Cliente atualizado com sucesso.";
        return RedirectToPage("Index");
    }
}
