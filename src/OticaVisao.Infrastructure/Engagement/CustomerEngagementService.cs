using System.Data;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Engagement;
using OticaVisao.Domain.Engagement;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Engagement;

public sealed class CustomerEngagementService(ApplicationDbContext context) : ICustomerEngagementService
{
    public async Task<IReadOnlySet<Guid>> FavoriteFrameIdsAsync(Guid accountUserId, CancellationToken ct = default)
    {
        var customerId = await CustomerIdAsync(accountUserId, ct);
        return (await context.FrameFavorites.AsNoTracking().Where(x => x.CustomerId == customerId).Select(x => x.FrameId).ToListAsync(ct)).ToHashSet();
    }

    public async Task<IReadOnlySet<Guid>> ActiveReservationFrameIdsAsync(Guid accountUserId, CancellationToken ct = default)
    {
        await ExpireStaleAsync(ct);
        var customerId = await CustomerIdAsync(accountUserId, ct);
        return (await context.FrameReservations.AsNoTracking().Where(x => x.CustomerId == customerId && x.Status == FrameReservationStatus.Active).Select(x => x.FrameId).ToListAsync(ct)).ToHashSet();
    }

    public async Task<IReadOnlyList<EngagementFrameItem>> ListFavoritesAsync(Guid accountUserId, CancellationToken ct = default)
    {
        var customerId = await CustomerIdAsync(accountUserId, ct);
        return await context.FrameFavorites.AsNoTracking().Where(x => x.CustomerId == customerId)
            .Join(context.Frames, favorite => favorite.FrameId, frame => frame.Id, (favorite, frame) => new { favorite, frame })
            .OrderByDescending(x => x.favorite.CreatedAtUtc)
            .Select(x => new EngagementFrameItem(x.frame.Id, x.frame.Code, x.frame.Brand, x.frame.Model, x.frame.Color, x.frame.Price, x.frame.ImageFileName, x.frame.StockQuantity - x.frame.ReservedQuantity)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ReservationItem>> ListReservationsAsync(Guid accountUserId, CancellationToken ct = default)
    {
        await ExpireStaleAsync(ct);
        var customerId = await CustomerIdAsync(accountUserId, ct);
        var rows = await (
            from reservation in context.FrameReservations.AsNoTracking()
            where reservation.CustomerId == customerId
            join customer in context.Customers on reservation.CustomerId equals customer.Id
            join frame in context.Frames on reservation.FrameId equals frame.Id
            orderby reservation.CreatedAtUtc descending
            select new { Reservation = reservation, CustomerName = customer.Name, CustomerPhone = customer.Phone, Frame = frame })
            .ToListAsync(ct);
        return rows.Select(x => Map(x.Reservation, x.CustomerName, x.CustomerPhone, x.Frame)).ToArray();
    }

    public async Task<IReadOnlyList<ReservationItem>> ListAllReservationsAsync(CancellationToken ct = default)
    {
        await ExpireStaleAsync(ct);
        var rows = await (
            from reservation in context.FrameReservations.AsNoTracking()
            join customer in context.Customers on reservation.CustomerId equals customer.Id
            join frame in context.Frames on reservation.FrameId equals frame.Id
            select new { Reservation = reservation, CustomerName = customer.Name, CustomerPhone = customer.Phone, Frame = frame })
            .ToListAsync(ct);
        return rows
            .OrderByDescending(x => x.Reservation.Status == FrameReservationStatus.Active)
            .ThenBy(x => x.Reservation.ExpiresAtUtc)
            .Select(x => Map(x.Reservation, x.CustomerName, x.CustomerPhone, x.Frame))
            .ToArray();
    }

    public async Task<bool> ToggleFavoriteAsync(Guid accountUserId, Guid frameId, CancellationToken ct = default)
    {
        var customerId = await CustomerIdAsync(accountUserId, ct);
        if (!await context.Frames.AnyAsync(x => x.Id == frameId && x.IsActive, ct)) throw new KeyNotFoundException("Armação não encontrada.");
        var favorite = await context.FrameFavorites.SingleOrDefaultAsync(x => x.CustomerId == customerId && x.FrameId == frameId, ct);
        if (favorite is null) { context.FrameFavorites.Add(new FrameFavorite(customerId, frameId)); await context.SaveChangesAsync(ct); return true; }
        context.FrameFavorites.Remove(favorite); await context.SaveChangesAsync(ct); return false;
    }

    public async Task<Guid> ReserveAsync(Guid accountUserId, Guid frameId, string? visitPeriod, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await ExpireStaleCoreAsync(ct);
        var customerId = await CustomerIdAsync(accountUserId, ct);
        if (await context.FrameReservations.CountAsync(x => x.CustomerId == customerId && x.Status == FrameReservationStatus.Active, ct) >= 3)
            throw new InvalidOperationException("Você pode manter no máximo 3 armações reservadas ao mesmo tempo.");
        if (await context.FrameReservations.AnyAsync(x => x.CustomerId == customerId && x.FrameId == frameId && x.Status == FrameReservationStatus.Active, ct))
            throw new InvalidOperationException("Esta armação já está nas suas reservas.");
        var frame = await context.Frames.SingleOrDefaultAsync(x => x.Id == frameId, ct) ?? throw new KeyNotFoundException("Armação não encontrada.");
        frame.ReserveOne();
        var reservation = new FrameReservation(customerId, frameId, visitPeriod);
        context.FrameReservations.Add(reservation);
        await context.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return reservation.Id;
    }

    public Task CancelAsync(Guid accountUserId, Guid reservationId, CancellationToken ct = default) => EndReservationAsync(reservationId, accountUserId, false, ct);
    public Task CancelByAdminAsync(Guid reservationId, CancellationToken ct = default) => EndReservationAsync(reservationId, null, false, ct);

    public async Task<Guid> ConvertToSaleAsync(Guid reservationId, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await ExpireStaleCoreAsync(ct);
        var reservation = await context.FrameReservations.SingleOrDefaultAsync(x => x.Id == reservationId, ct) ?? throw new KeyNotFoundException("Reserva não encontrada.");
        if (reservation.Status != FrameReservationStatus.Active) throw new InvalidOperationException("Esta reserva não está ativa.");
        var frame = await context.Frames.SingleAsync(x => x.Id == reservation.FrameId, ct);
        var sale = new Sale(reservation.CustomerId);
        frame.ReleaseReservation();
        context.Sales.Add(sale);
        reservation.ConvertToSale(sale.Id);
        await context.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
        return sale.Id;
    }

    public async Task<int> ExpireStaleAsync(CancellationToken ct = default)
    {
        var count = await ExpireStaleCoreAsync(ct);
        if (count > 0) await context.SaveChangesAsync(ct);
        return count;
    }

    private async Task EndReservationAsync(Guid reservationId, Guid? accountUserId, bool expire, CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var reservation = await context.FrameReservations.SingleOrDefaultAsync(x => x.Id == reservationId, ct) ?? throw new KeyNotFoundException("Reserva não encontrada.");
        if (accountUserId.HasValue && reservation.CustomerId != await CustomerIdAsync(accountUserId.Value, ct)) throw new UnauthorizedAccessException();
        if (reservation.Status != FrameReservationStatus.Active) throw new InvalidOperationException("Esta reserva já foi encerrada.");
        var frame = await context.Frames.SingleAsync(x => x.Id == reservation.FrameId, ct);
        frame.ReleaseReservation(); if (expire) reservation.Expire(); else reservation.Cancel();
        await context.SaveChangesAsync(ct); await transaction.CommitAsync(ct);
    }

    private async Task<int> ExpireStaleCoreAsync(CancellationToken ct)
    {
        var expired = await context.FrameReservations.Where(x => x.Status == FrameReservationStatus.Active && x.ExpiresAtUtc <= DateTime.UtcNow).ToListAsync(ct);
        if (expired.Count == 0) return 0;
        var frameIds = expired.Select(x => x.FrameId).Distinct().ToArray();
        var frames = await context.Frames.Where(x => frameIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, ct);
        foreach (var reservation in expired) { if (frames.TryGetValue(reservation.FrameId, out var frame) && frame.ReservedQuantity > 0) frame.ReleaseReservation(); reservation.Expire(); }
        return expired.Count;
    }

    private async Task<Guid> CustomerIdAsync(Guid accountUserId, CancellationToken ct) =>
        await context.Customers.Where(x => x.AccountUserId == accountUserId).Select(x => x.Id).SingleOrDefaultAsync(ct) is var id && id != Guid.Empty ? id : throw new InvalidOperationException("Complete seu cadastro antes de usar este recurso.");

    private static ReservationItem Map(FrameReservation r, string name, string phone, Domain.Catalog.Frame f) => new(r.Id, r.CustomerId, name, phone, new(f.Id, f.Code, f.Brand, f.Model, f.Color, f.Price, f.ImageFileName, f.AvailableQuantity), r.Status, r.VisitPeriod, r.CreatedAtUtc, r.ExpiresAtUtc, r.EndedAtUtc, r.SaleId);
}
