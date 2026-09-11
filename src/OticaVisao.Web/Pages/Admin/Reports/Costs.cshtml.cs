using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Costs;
using OticaVisao.Domain.Costs;
namespace OticaVisao.Web.Pages.Admin.Reports;
public sealed class CostsModel(CostService service) : PageModel
{
    [BindProperty(SupportsGet = true)] public string? Month { get; set; }
    [BindProperty, Required] public CostCategory? Category { get; set; }
    [BindProperty] public decimal? Amount { get; set; }
    [BindProperty, Required, StringLength(200)] public string? Description { get; set; }
    [BindProperty, DataType(DataType.Date)] public DateOnly? Date { get; set; }
    [BindProperty] public decimal? Budget { get; set; }
    public CostReport Report { get; private set; } = null!;
    public async Task OnGetAsync(CancellationToken ct) { var month = ParseMonth(); Month = month.ToString("yyyy-MM", CultureInfo.InvariantCulture); Date ??= DateOnly.FromDateTime(DateTime.Today); Report = await service.GetAsync(month, ct); Budget = Report.Budget; }
    public async Task<IActionResult> OnPostAddAsync(CancellationToken ct) { var month = ParseMonth(); if (!Category.HasValue || !Amount.HasValue || Amount <= 0 || Amount > 99_999_999 || !Date.HasValue || string.IsNullOrWhiteSpace(Description)) { TempData["CostError"] = "Preencha os dados da despesa com um valor maior que zero."; return RedirectToPage(new { month = month.ToString("yyyy-MM", CultureInfo.InvariantCulture) }); } await service.AddAsync(Category.Value, Amount.Value, Date.Value, Description, ct); TempData["CostMessage"] = "Despesa registrada."; return RedirectToPage(new { month = month.ToString("yyyy-MM", CultureInfo.InvariantCulture) }); }
    public async Task<IActionResult> OnPostBudgetAsync(CancellationToken ct) { var month = ParseMonth(); if (!Budget.HasValue || Budget < 0 || Budget > 99_999_999) { TempData["CostError"] = "Informe um orçamento válido."; return RedirectToPage(new { month = month.ToString("yyyy-MM", CultureInfo.InvariantCulture) }); } await service.SetBudgetAsync(month, Budget.Value, ct); TempData["CostMessage"] = "Orçamento atualizado."; return RedirectToPage(new { month = month.ToString("yyyy-MM", CultureInfo.InvariantCulture) }); }
    private DateOnly ParseMonth() => DateOnly.TryParseExact(Month, "yyyy-MM", out var value) ? new(value.Year, value.Month, 1) : new(DateTime.Today.Year, DateTime.Today.Month, 1);
}
