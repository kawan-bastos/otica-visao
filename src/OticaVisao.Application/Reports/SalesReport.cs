using OticaVisao.Domain.Sales;

namespace OticaVisao.Application.Reports;

public sealed record SalesReport(
    DateOnly From, DateOnly To, int CompletedSales, int CancelledSales, int ReversedSales,
    decimal Revenue, decimal AverageTicket, int SoldUnits, int CompleteGlasses,
    int UniqueCustomers, int DraftSales,
    IReadOnlyList<PaymentReportItem> Payments,
    IReadOnlyList<TopFrameReportItem> TopFrames,
    IReadOnlyList<DailySalesReportItem> DailySales,
    IReadOnlyList<LowStockReportItem> LowStock);

public sealed record PaymentReportItem(PaymentMethod Method, int Sales, decimal Total);
public sealed record TopFrameReportItem(string Code, string Description, int Quantity, decimal Total);
public sealed record DailySalesReportItem(DateOnly Date, int Sales, int SoldUnits, decimal Total);
public sealed record LowStockReportItem(Guid FrameId, string Code, string Description, int StockQuantity);
