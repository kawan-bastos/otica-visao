using OticaVisao.Domain.Catalog;

namespace OticaVisao.Application.Catalog;

public sealed record PublicFrameFilter(
    string? Search = null,
    FrameType? Type = null,
    FrameShape? Shape = null,
    TargetAudience? TargetAudience = null);
