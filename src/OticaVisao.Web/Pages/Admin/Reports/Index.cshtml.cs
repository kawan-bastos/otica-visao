using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Reports;

namespace OticaVisao.Web.Pages.Admin.Reports;

public sealed class IndexModel(ReportService reportService) : PageModel
{
    private static readonly int[] AllowedRanges = [1, 7, 30, 90];

    [BindProperty(SupportsGet = true)] public int Days { get; set; } = 30;
    public SalesReport Report { get; private set; } = null!;
    public SalesReport PreviousReport { get; private set; } = null!;
    public string RangeLabel => Days == 1 ? "Hoje" : $"Últimos {Days} dias";
    public string ComparisonLabel => Days == 1 ? "comparado a ontem" : $"comparado aos {Days} dias anteriores";

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        NormalizeRange();
        (Report, PreviousReport) = await LoadReportsAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken cancellationToken)
    {
        NormalizeRange();
        var (report, _) = await LoadReportsAsync(cancellationToken);
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var csv = new StringBuilder("Data;Vendas;Unidades;Faturamento\r\n");
        foreach (var day in report.DailySales)
            csv.Append(day.Date.ToString("dd/MM/yyyy", culture)).Append(';').Append(day.Sales).Append(';')
                .Append(day.SoldUnits).Append(';').Append(day.Total.ToString("0.00", culture)).Append("\r\n");
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),
            "text/csv; charset=utf-8", $"relatorio-{report.From:yyyy-MM-dd}-{report.To:yyyy-MM-dd}.csv");
    }

    private async Task<(SalesReport Current, SalesReport Previous)> LoadReportsAsync(CancellationToken cancellationToken)
    {
        var to = DateOnly.FromDateTime(DateTime.Today);
        var from = to.AddDays(-(Days - 1));
        var previousTo = from.AddDays(-1);
        var previousFrom = previousTo.AddDays(-(Days - 1));
        return (await reportService.GetSalesReportAsync(from, to, cancellationToken),
            await reportService.GetSalesReportAsync(previousFrom, previousTo, cancellationToken));
    }

    private void NormalizeRange() { if (!AllowedRanges.Contains(Days)) Days = 30; }
}
