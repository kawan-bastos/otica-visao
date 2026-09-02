using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public interface IFrameRepository
{
    Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default);

    Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(Frame frame, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
