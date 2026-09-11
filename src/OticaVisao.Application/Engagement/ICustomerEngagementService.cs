using OticaVisao.Domain.Engagement;

namespace OticaVisao.Application.Engagement;

public sealed record EngagementFrameItem(Guid Id, string Code, string Brand, string Model, string Color, decimal Price, string? ImageFileName, int AvailableQuantity);
public sealed record ReservationItem(Guid Id, Guid CustomerId, string CustomerName, string CustomerPhone, EngagementFrameItem Frame, FrameReservationStatus Status, string? VisitPeriod, DateTime CreatedAtUtc, DateTime ExpiresAtUtc, DateTime? EndedAtUtc, Guid? SaleId);

public interface ICustomerEngagementService
{
    Task<IReadOnlySet<Guid>> FavoriteFrameIdsAsync(Guid accountUserId, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> ActiveReservationFrameIdsAsync(Guid accountUserId, CancellationToken ct = default);
    Task<IReadOnlyList<EngagementFrameItem>> ListFavoritesAsync(Guid accountUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationItem>> ListReservationsAsync(Guid accountUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationItem>> ListAllReservationsAsync(CancellationToken ct = default);
    Task<bool> ToggleFavoriteAsync(Guid accountUserId, Guid frameId, CancellationToken ct = default);
    Task<Guid> ReserveAsync(Guid accountUserId, Guid frameId, string? visitPeriod, CancellationToken ct = default);
    Task CancelAsync(Guid accountUserId, Guid reservationId, CancellationToken ct = default);
    Task CancelByAdminAsync(Guid reservationId, CancellationToken ct = default);
    Task<Guid> ConvertToSaleAsync(Guid reservationId, CancellationToken ct = default);
    Task<int> ExpireStaleAsync(CancellationToken ct = default);
}
