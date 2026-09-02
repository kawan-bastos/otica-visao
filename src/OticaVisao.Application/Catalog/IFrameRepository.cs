using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public interface IFrameRepository
{
    Task<IReadOnlyList<Frame>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Frame>> ListPublicAsync(
        PublicFrameFilter filter,
        int? maximumItems = null,
        CancellationToken cancellationToken = default);

    Task<Frame?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Frame?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> CodeExistsAsync(
        string code,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Frame frame, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
