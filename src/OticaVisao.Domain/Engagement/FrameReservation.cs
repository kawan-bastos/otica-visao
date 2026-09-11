namespace OticaVisao.Domain.Engagement;

public enum FrameReservationStatus { Active, Cancelled, Expired, ConvertedToSale }

public sealed class FrameReservation
{
    private FrameReservation() { }
    public FrameReservation(Guid customerId, Guid frameId, string? visitPeriod = null)
    {
        Id = Guid.NewGuid(); CustomerId = customerId; FrameId = frameId;
        VisitPeriod = NormalizeVisitPeriod(visitPeriod); Status = FrameReservationStatus.Active;
        CreatedAtUtc = DateTime.UtcNow; ExpiresAtUtc = CreatedAtUtc.AddHours(24);
    }
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid FrameId { get; private set; }
    public FrameReservationStatus Status { get; private set; }
    public string? VisitPeriod { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public Guid? SaleId { get; private set; }
    public void Cancel() => End(FrameReservationStatus.Cancelled);
    public void Expire() => End(FrameReservationStatus.Expired);
    public void ConvertToSale(Guid saleId) { if (saleId == Guid.Empty) throw new ArgumentException("Venda inválida.", nameof(saleId)); End(FrameReservationStatus.ConvertedToSale); SaleId = saleId; }
    private void End(FrameReservationStatus status) { if (Status != FrameReservationStatus.Active) throw new InvalidOperationException("Esta reserva já foi encerrada."); Status = status; EndedAtUtc = DateTime.UtcNow; }
    private static string? NormalizeVisitPeriod(string? value) { if (string.IsNullOrWhiteSpace(value)) return null; var normalized = value.Trim(); if (normalized.Length > 40) throw new ArgumentException("O período deve possuir no máximo 40 caracteres.", nameof(value)); return normalized; }
}
