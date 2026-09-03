using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Reports;

namespace OticaVisao.Web.Pages.Admin.Reports;

public sealed class IndexModel(ReportService reportService) : PageModel
{
    [BindProperty(SupportsGet = true)] public DateOnly? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? To { get; set; }
    public SalesReport Report { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        From ??= new DateOnly(today.Year, today.Month, 1);
        To ??= today;
        try
        {
            Report = await reportService.GetSalesReportAsync(From.Value, To.Value, cancellationToken);
        }
        catch (ArgumentException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            From = new DateOnly(today.Year, today.Month, 1);
            To = today;
            Report = await reportService.GetSalesReportAsync(From.Value, To.Value, cancellationToken);
        }
    }
}
