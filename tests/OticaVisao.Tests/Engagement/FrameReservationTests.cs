using OticaVisao.Domain.Engagement;

namespace OticaVisao.Tests.Engagement;

public sealed class FrameReservationTests
{
    [Fact]
    public void NewReservationIsActiveForTwentyFourHours()
    {
        var reservation = new FrameReservation(Guid.NewGuid(), Guid.NewGuid(), " Manhã ");

        Assert.Equal(FrameReservationStatus.Active, reservation.Status);
        Assert.Equal("Manhã", reservation.VisitPeriod);
        Assert.InRange(reservation.ExpiresAtUtc - reservation.CreatedAtUtc, TimeSpan.FromHours(24), TimeSpan.FromHours(24));
    }

    [Fact]
    public void ConvertedReservationKeepsSaleReference()
    {
        var reservation = new FrameReservation(Guid.NewGuid(), Guid.NewGuid());
        var saleId = Guid.NewGuid();

        reservation.ConvertToSale(saleId);

        Assert.Equal(FrameReservationStatus.ConvertedToSale, reservation.Status);
        Assert.Equal(saleId, reservation.SaleId);
        Assert.NotNull(reservation.EndedAtUtc);
    }

    [Fact]
    public void ClosedReservationCannotBeClosedTwice()
    {
        var reservation = new FrameReservation(Guid.NewGuid(), Guid.NewGuid());
        reservation.Cancel();

        Assert.Throws<InvalidOperationException>(reservation.Expire);
    }
}
