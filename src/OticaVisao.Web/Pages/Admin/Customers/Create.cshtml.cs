using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Customers;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Customers;

public sealed class CreateModel(CustomerService customerService) : PageModel
{
    [BindProperty] public CustomerInputModel Input { get; set; } = new();
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await customerService.CreateAsync(Input.ToCreateRequest(), cancellationToken);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return Page();
        }
        TempData["SuccessMessage"] = "Cliente cadastrado com sucesso.";
        return RedirectToPage("Index");
    }
}
