namespace OticaVisao.Domain.Engagement;

public sealed class FrameFavorite
{
    private FrameFavorite() { }
    public FrameFavorite(Guid customerId, Guid frameId)
    {
        Id = Guid.NewGuid(); CustomerId = customerId; FrameId = frameId; CreatedAtUtc = DateTime.UtcNow;
    }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid FrameId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
