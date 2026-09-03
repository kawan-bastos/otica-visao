using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.Sales;

namespace OticaVisao.Web.Models.Admin;

public sealed class PrescriptionInputModel
{
    public PrescriptionEyeInput FarRight { get; set; } = new();
    public PrescriptionEyeInput FarLeft { get; set; } = new();
    public PrescriptionEyeInput NearRight { get; set; } = new();
    public PrescriptionEyeInput NearLeft { get; set; } = new();

    public LensPrescription ToDomain() => new(FarRight.ToDomain(), FarLeft.ToDomain(), NearRight.ToDomain(), NearLeft.ToDomain());

    public static PrescriptionInputModel From(LensPrescription value) => new()
    {
        FarRight = PrescriptionEyeInput.From(value.FarRight), FarLeft = PrescriptionEyeInput.From(value.FarLeft),
        NearRight = PrescriptionEyeInput.From(value.NearRight), NearLeft = PrescriptionEyeInput.From(value.NearLeft)
    };
}

public sealed class PrescriptionEyeInput
{
    [Range(typeof(decimal), "-30", "30", ErrorMessage = "Use um valor entre -30 e 30.")] public decimal? Sphere { get; set; }
    [Range(typeof(decimal), "-30", "30", ErrorMessage = "Use um valor entre -30 e 30.")] public decimal? Cylinder { get; set; }
    [Range(0, 180, ErrorMessage = "O eixo deve estar entre 0 e 180.")] public int? Axis { get; set; }
    [Range(typeof(decimal), "20", "80", ErrorMessage = "A DNP deve estar entre 20 e 80 mm.")] public decimal? Dnp { get; set; }
    [Range(typeof(decimal), "5", "60", ErrorMessage = "A altura deve estar entre 5 e 60 mm.")] public decimal? Height { get; set; }
    [Range(typeof(decimal), "-30", "30", ErrorMessage = "Use um valor entre -30 e 30.")] public decimal? Addition { get; set; }

    public PrescriptionEyeValues ToDomain() => new(Sphere, Cylinder, Axis, Dnp, Height, Addition);
    public static PrescriptionEyeInput From(PrescriptionEyeValues value) => new() { Sphere = value.Sphere, Cylinder = value.Cylinder, Axis = value.Axis, Dnp = value.Dnp, Height = value.Height, Addition = value.Addition };
}
