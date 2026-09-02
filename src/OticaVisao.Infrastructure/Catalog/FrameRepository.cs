using Microsoft.EntityFrameworkCore;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Infrastructure.Catalog;

public sealed class FrameRepository(ApplicationDbContext context) : IFrameRepository
{
    public async Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await context.Frames
            .AsNoTracking()
            .OrderBy(frame => frame.Brand)
            .ThenBy(frame => frame.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Frame>> ListPublicAsync(
        PublicFrameFilter filter,
        int? maximumItems = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Frames
            .AsNoTracking()
            .Where(frame => frame.IsActive && frame.IsPublished && frame.StockQuantity > 0);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = $"%{filter.Search.Trim()}%";
            query = query.Where(frame =>
                EF.Functions.ILike(frame.Brand, search)
                || EF.Functions.ILike(frame.Model, search)
                || EF.Functions.ILike(frame.Color, search)
                || EF.Functions.ILike(frame.Code, search));
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(frame => frame.Type == filter.Type.Value);
        }

        if (filter.Shape.HasValue)
        {
            query = query.Where(frame => frame.Shape == filter.Shape.Value);
        }

        if (filter.TargetAudience.HasValue)
        {
            query = query.Where(frame => frame.TargetAudience == filter.TargetAudience.Value);
        }

        query = query.OrderBy(frame => frame.Brand).ThenBy(frame => frame.Model);
        if (maximumItems.HasValue)
        {
            query = query.Take(Math.Clamp(maximumItems.Value, 1, 100));
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Frames.SingleOrDefaultAsync(frame => frame.Id == id, cancellationToken);
    }

    public Task<Frame?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Frames.AsNoTracking().SingleOrDefaultAsync(
            frame => frame.Id == id && frame.IsActive && frame.IsPublished && frame.StockQuantity > 0,
            cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        string code,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        return context.Frames.AnyAsync(
            frame => frame.Code == code && (!excludingId.HasValue || frame.Id != excludingId.Value),
            cancellationToken);
    }

    public Task AddAsync(Frame frame, CancellationToken cancellationToken = default)
    {
        return context.Frames.AddAsync(frame, cancellationToken).AsTask();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
