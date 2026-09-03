using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Web.Pages.Admin.Sales;
public sealed class EditItemModel(SaleService service) : PageModel
{
    [BindProperty] public EditSaleItemLensInputModel Input { get; set; } = new();
    public SaleListItem Sale { get; private set; } = null!;
    public SaleItemListItem Item { get; private set; } = null!;
    public async Task<IActionResult> OnGetAsync(Guid saleId, Guid itemId, CancellationToken token) { if (!await Load(saleId,itemId,token)) return NotFound(); Input = new() { LensDescription=Item.LensDescription, LensUnitPrice=Item.LensUnitPrice, Laboratory=Item.Laboratory, Prescription=PrescriptionInputModel.From(Item.Prescription), ExpectedDeliveryDate=Item.ExpectedDeliveryDate }; return Page(); }
    public async Task<IActionResult> OnPostAsync(Guid saleId, Guid itemId, CancellationToken token) { if (!ModelState.IsValid) { if (!await Load(saleId,itemId,token)) return NotFound(); return Page(); } try { await service.UpdateItemLensAsync(saleId,itemId,Input.ToRequest(),token); TempData["SaleSuccessMessage"]="Lentes e grau atualizados."; return RedirectToPage("Details",new{id=saleId}); } catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or KeyNotFoundException) { ModelState.AddModelError(string.Empty,ex.Message); if(!await Load(saleId,itemId,token)) return NotFound(); return Page(); } }
    private async Task<bool> Load(Guid saleId, Guid itemId, CancellationToken token) { var sale=await service.GetAsync(saleId,token); var item=sale?.Items.SingleOrDefault(x=>x.Id==itemId); if(sale is null||item is null||!item.IncludesLenses||sale.Status is SaleStatus.Cancelled or SaleStatus.Reversed)return false; Sale=sale;Item=item;return true; }
}
