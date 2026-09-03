using OticaVisao.Application.Catalog;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Reports;

public sealed class ReportService(ISaleRepository saleRepository, IFrameRepository frameRepository)
{
    private static readonly TimeZoneInfo StoreTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public async Task<SalesReport> GetSalesReportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (from > to) throw new ArgumentException("A data inicial não pode ser posterior à data final.");
        if (to.DayNumber - from.DayNumber > 366) throw new ArgumentException("Selecione um período de até 366 dias.");

        var allSales = await saleRepository.ListAsync(cancellationToken);
        var completed = allSales.Where(sale => sale.Status == SaleStatus.Completed && sale.CompletedAtUtc.HasValue)
            .Where(sale => IsInPeriod(sale.CompletedAtUtc!.Value, from, to)).ToArray();
        var cancelled = allSales.Count(sale => sale.Status == SaleStatus.Cancelled && sale.CancelledAtUtc.HasValue
            && IsInPeriod(sale.CancelledAtUtc.Value, from, to));
        var revenue = completed.Sum(sale => sale.FinalTotal ?? sale.Total);

        var payments = completed.Where(sale => sale.PaymentMethod.HasValue)
            .GroupBy(sale => sale.PaymentMethod!.Value)
            .Select(group => new PaymentReportItem(group.Key, group.Count(), group.Sum(sale => sale.FinalTotal ?? sale.Total)))
            .OrderByDescending(item => item.Total).ToArray();

        var topFrames = completed.SelectMany(sale => sale.Items)
            .GroupBy(item => new { item.FrameCode, item.FrameBrand, item.FrameModel })
            .Select(group => new TopFrameReportItem(group.Key.FrameCode, $"{group.Key.FrameBrand} {group.Key.FrameModel}",
                group.Sum(item => item.Quantity), group.Sum(item => item.Total)))
            .OrderByDescending(item => item.Quantity).ThenByDescending(item => item.Total).Take(8).ToArray();

        var dailySales = completed.GroupBy(sale => LocalDate(sale.CompletedAtUtc!.Value))
            .Select(group => new DailySalesReportItem(group.Key, group.Count(), group.Sum(sale => sale.FinalTotal ?? sale.Total)))
            .OrderBy(item => item.Date).ToArray();

        var lowStock = (await frameRepository.ListAsync(cancellationToken))
            .Where(frame => frame.IsActive && frame.StockQuantity <= 2)
            .OrderBy(frame => frame.StockQuantity).ThenBy(frame => frame.Code)
            .Select(frame => new LowStockReportItem(frame.Id, frame.Code, $"{frame.Brand} {frame.Model}", frame.StockQuantity)).ToArray();

        return new SalesReport(from, to, completed.Length, cancelled, revenue,
            completed.Length == 0 ? 0 : revenue / completed.Length,
            completed.SelectMany(sale => sale.Items).Sum(item => item.Quantity),
            completed.SelectMany(sale => sale.Items).Where(item => item.IncludesLenses).Sum(item => item.Quantity),
            completed.Select(sale => sale.CustomerId).Distinct().Count(),
            allSales.Count(sale => sale.Status == SaleStatus.Draft), payments, topFrames, dailySales, lowStock);
    }

    private static bool IsInPeriod(DateTimeOffset date, DateOnly from, DateOnly to)
    {
        var local = LocalDate(date);
        return local >= from && local <= to;
    }

    private static DateOnly LocalDate(DateTimeOffset date) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(date, StoreTimeZone).DateTime);
}
