using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OticaVisao.Application.Engagement;
using OticaVisao.Application.Sales;
using OticaVisao.Domain.Engagement;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Pages.Admin.Reservations;

public sealed class IndexModel(ICustomerEngagementService service, SaleService saleService) : PageModel
{
    private Dictionary<Guid, SaleStatus> saleStatuses = [];
    public sealed record PopularFrame(string Code, string Name, int Reservations);

    public IReadOnlyList<ReservationItem> Reservations { get; private set; } = [];
    public IReadOnlyList<PopularFrame> PopularFrames { get; private set; } = [];
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? Date { get; set; }
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "newest";
    public int TotalReservations { get; private set; }
    public int ActiveReservations { get; private set; }
    public int TodayReservations { get; private set; }
    public int ConvertedReservations { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var all = await service.ListAllReservationsAsync(ct);
        saleStatuses = (await saleService.ListAsync(ct)).ToDictionary(sale => sale.Id, sale => sale.Status);
        TotalReservations = all.Count;
        ActiveReservations = all.Count(item => item.Status == FrameReservationStatus.Active);
        TodayReservations = all.Count(item => item.CreatedAtUtc.ToLocalTime().Date == DateTime.Today);
        ConvertedReservations = all.Count(item => IsSaleWithStatus(item, SaleStatus.Completed));
        PopularFrames = all.GroupBy(item => new { item.Frame.Code, item.Frame.Brand, item.Frame.Model })
            .Select(group => new PopularFrame(group.Key.Code, $"{group.Key.Brand} {group.Key.Model}", group.Count()))
            .OrderByDescending(item => item.Reservations).ThenBy(item => item.Name).Take(5).ToArray();

        IEnumerable<ReservationItem> filtered = all;
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            filtered = filtered.Where(item => item.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.CustomerPhone.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Frame.Code.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Frame.Brand.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Frame.Model.Contains(term, StringComparison.OrdinalIgnoreCase)
                || item.Frame.Color.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        filtered = Status switch
        {
            "active" => filtered.Where(item => item.Status == FrameReservationStatus.Active),
            "expired" => filtered.Where(item => item.Status == FrameReservationStatus.Expired),
            "cancelled" => filtered.Where(item => item.Status == FrameReservationStatus.Cancelled),
            "converted" => filtered.Where(item => IsSaleWithStatus(item, SaleStatus.Completed)),
            "reversed" => filtered.Where(item => IsSaleWithStatus(item, SaleStatus.Reversed)),
            _ => filtered
        };
        if (Date.HasValue) filtered = filtered.Where(item => item.CreatedAtUtc.ToLocalTime().Date == Date.Value.Date);

        var popularity = all.GroupBy(item => item.Frame.Id).ToDictionary(group => group.Key, group => group.Count());
        filtered = Sort switch
        {
            "oldest" => filtered.OrderBy(item => item.CreatedAtUtc),
            "expiring" => filtered.OrderBy(item => item.Status != FrameReservationStatus.Active).ThenBy(item => item.ExpiresAtUtc),
            "most-reserved" => filtered.OrderByDescending(item => popularity[item.Frame.Id]).ThenByDescending(item => item.CreatedAtUtc),
            _ => filtered.OrderByDescending(item => item.CreatedAtUtc)
        };
        Reservations = filtered.ToArray();
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id, CancellationToken ct)
    {
        try { await service.CancelByAdminAsync(id, ct); TempData["ReservationMessage"] = "Reserva cancelada."; }
        catch (InvalidOperationException ex) { TempData["ReservationError"] = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConvertAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var reservation = (await service.ListAllReservationsAsync(ct)).SingleOrDefault(item => item.Id == id)
                ?? throw new KeyNotFoundException("Reserva não encontrada.");
            var saleId = await service.ConvertToSaleAsync(id, ct);
            TempData["SaleSuccessMessage"] = "Armação selecionada a partir da reserva. Informe os valores cobrados antes de finalizar.";
            return RedirectToPage("/Admin/Sales/Details", new { id = saleId, frameId = reservation.Frame.Id, fromReservation = true });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ReservationError"] = ex.Message;
            return RedirectToPage();
        }
    }

    public string StatusName(ReservationItem reservation)
    {
        if (reservation.SaleId is Guid saleId && saleStatuses.TryGetValue(saleId, out var saleStatus))
        {
            return saleStatus switch
            {
                SaleStatus.Draft => "Venda em andamento",
                SaleStatus.Completed => "Venda concluída",
                SaleStatus.Cancelled => "Venda cancelada",
                SaleStatus.Reversed => "Venda estornada",
                _ => "Convertida em venda"
            };
        }

        return reservation.Status switch
        {
            FrameReservationStatus.Active => "Ativa",
            FrameReservationStatus.Cancelled => "Cancelada",
            FrameReservationStatus.Expired => "Expirada",
            _ => "Convertida em venda"
        };
    }

    private bool IsSaleWithStatus(ReservationItem reservation, SaleStatus status) =>
        reservation.SaleId is Guid saleId
        && saleStatuses.TryGetValue(saleId, out var actualStatus)
        && actualStatus == status;
}
