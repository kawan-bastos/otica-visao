namespace OticaVisao.Domain.Sales;

public sealed record LensPrescription(
    PrescriptionEyeValues FarRight,
    PrescriptionEyeValues FarLeft,
    PrescriptionEyeValues NearRight,
    PrescriptionEyeValues NearLeft)
{
    public static LensPrescription Empty { get; } = new(
        PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty,
        PrescriptionEyeValues.Empty, PrescriptionEyeValues.Empty);

    public LensPrescription Validate()
    {
        FarRight.Validate();
        FarLeft.Validate();
        NearRight.Validate();
        NearLeft.Validate();
        return this;
    }
}

public sealed record PrescriptionEyeValues(
    decimal? Sphere,
    decimal? Cylinder,
    int? Axis,
    decimal? Dnp,
    decimal? Height,
    decimal? Addition)
{
    public static PrescriptionEyeValues Empty { get; } = new(null, null, null, null, null, null);
    public bool HasAnyValue => Sphere.HasValue || Cylinder.HasValue || Axis.HasValue
        || Dnp.HasValue || Height.HasValue || Addition.HasValue;

    internal void Validate()
    {
        ValidatePower(Sphere, "esférico");
        ValidatePower(Cylinder, "cilíndrico");
        ValidatePower(Addition, "adição");
        if (Axis is < 0 or > 180) throw new ArgumentOutOfRangeException(nameof(Axis), "O eixo deve estar entre 0 e 180 graus.");
        if (Dnp is < 20 or > 80) throw new ArgumentOutOfRangeException(nameof(Dnp), "A DNP deve estar entre 20 e 80 mm.");
        if (Height is < 5 or > 60) throw new ArgumentOutOfRangeException(nameof(Height), "A altura deve estar entre 5 e 60 mm.");
    }

    private static void ValidatePower(decimal? value, string field)
    {
        if (value is < -30 or > 30) throw new ArgumentOutOfRangeException(field, $"O valor {field} deve estar entre -30 e +30.");
    }
}
