namespace OticaVisao.Domain.Catalog;

public sealed record FrameMeasurements
{
    public FrameMeasurements(int lensWidthMillimeters, int bridgeWidthMillimeters, int templeLengthMillimeters)
    {
        LensWidthMillimeters = Validate(lensWidthMillimeters, nameof(lensWidthMillimeters));
        BridgeWidthMillimeters = Validate(bridgeWidthMillimeters, nameof(bridgeWidthMillimeters));
        TempleLengthMillimeters = Validate(templeLengthMillimeters, nameof(templeLengthMillimeters));
    }

    public int LensWidthMillimeters { get; }

    public int BridgeWidthMillimeters { get; }

    public int TempleLengthMillimeters { get; }

    private static int Validate(int value, string parameterName)
    {
        if (value is <= 0 or > 300)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "A medida deve estar entre 1 e 300 milímetros.");
        }

        return value;
    }
}
