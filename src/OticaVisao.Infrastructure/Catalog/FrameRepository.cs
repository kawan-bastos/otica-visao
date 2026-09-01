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

    public Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Frames.SingleOrDefaultAsync(frame => frame.Id == id, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return context.Frames.AnyAsync(frame => frame.Code == code, cancellationToken);
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
