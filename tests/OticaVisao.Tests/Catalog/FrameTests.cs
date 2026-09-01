using OticaVisao.Domain.Catalog;

namespace OticaVisao.Tests.Catalog;

public sealed class FrameTests
{
    [Fact]
    public void NewFrameStartsActiveAndUnpublished()
    {
        var frame = CreateFrame(stockQuantity: 2);

        Assert.NotEqual(Guid.Empty, frame.Id);
        Assert.True(frame.IsActive);
        Assert.False(frame.IsPublished);
        Assert.False(frame.IsAvailable);
    }

    [Fact]
    public void PublishedFrameWithStockIsAvailable()
    {
        var frame = CreateFrame(stockQuantity: 2);

        frame.Publish();

        Assert.True(frame.IsAvailable);
    }

    [Fact]
    public void RemovingLastUnitMakesFrameUnavailable()
    {
        var frame = CreateFrame(stockQuantity: 1);
        frame.Publish();

        frame.RemoveFromStock();

        Assert.Equal(0, frame.StockQuantity);
        Assert.False(frame.IsAvailable);
    }

    [Fact]
    public void CannotRemoveMoreUnitsThanAvailable()
    {
        var frame = CreateFrame(stockQuantity: 1);

        var action = () => frame.RemoveFromStock(2);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("Não há estoque suficiente para realizar a baixa.", exception.Message);
    }

    [Fact]
    public void DeactivatingFrameAlsoUnpublishesIt()
    {
        var frame = CreateFrame(stockQuantity: 2);
        frame.Publish();

        frame.Deactivate();

        Assert.False(frame.IsActive);
        Assert.False(frame.IsPublished);
        Assert.False(frame.IsAvailable);
    }

    [Fact]
    public void CannotPublishDeactivatedFrame()
    {
        var frame = CreateFrame(stockQuantity: 2);
        frame.Deactivate();

        var action = frame.Publish;

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void NegativeInitialStockIsRejected()
    {
        var action = () => CreateFrame(stockQuantity: -1);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void TextFieldsAreTrimmed()
    {
        var frame = new Frame(
            " OV-1001 ",
            " Marca ",
            " Modelo ",
            " Preto ",
            219m,
            1,
            FrameType.Prescription,
            FrameShape.Square,
            TargetAudience.Adult);

        Assert.Equal("OV-1001", frame.Code);
        Assert.Equal("Marca", frame.Brand);
        Assert.Equal("Modelo", frame.Model);
        Assert.Equal("Preto", frame.Color);
    }

    private static Frame CreateFrame(int stockQuantity) =>
        new(
            "OV-1001",
            "Modelo demonstrativo",
            "Quadrado",
            "Preto",
            219m,
            stockQuantity,
            FrameType.Prescription,
            FrameShape.Square,
            TargetAudience.Adult,
            new FrameMeasurements(52, 18, 140));
}
