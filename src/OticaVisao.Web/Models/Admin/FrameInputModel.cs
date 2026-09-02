using System.ComponentModel.DataAnnotations;
using OticaVisao.Application.Catalog;
using OticaVisao.Domain.Catalog;

namespace OticaVisao.Web.Models.Admin;

public sealed class FrameInputModel : IValidatableObject
{
    [Required(ErrorMessage = "Informe o código.")]
    [StringLength(40, ErrorMessage = "O código deve ter no máximo 40 caracteres.")]
    [Display(Name = "Código")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a marca.")]
    [StringLength(100, ErrorMessage = "A marca deve ter no máximo 100 caracteres.")]
    [Display(Name = "Marca")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o modelo.")]
    [StringLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres.")]
    [Display(Name = "Modelo")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cor.")]
    [StringLength(80, ErrorMessage = "A cor deve ter no máximo 80 caracteres.")]
    [Display(Name = "Cor")]
    public string Color { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999", ErrorMessage = "Informe um preço válido.")]
    [Display(Name = "Preço")]
    public decimal Price { get; set; } = 219m;

    [Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
    [Display(Name = "Quantidade em estoque")]
    public int StockQuantity { get; set; }

    [Display(Name = "Tipo")]
    [EnumDataType(typeof(FrameType), ErrorMessage = "Selecione um tipo válido.")]
    public FrameType Type { get; set; } = FrameType.Prescription;

    [Display(Name = "Formato")]
    [EnumDataType(typeof(FrameShape), ErrorMessage = "Selecione um formato válido.")]
    public FrameShape Shape { get; set; }

    [Display(Name = "Público")]
    [EnumDataType(typeof(TargetAudience), ErrorMessage = "Selecione um público válido.")]
    public TargetAudience TargetAudience { get; set; } = TargetAudience.Adult;

    [Range(1, 300, ErrorMessage = "A largura deve estar entre 1 e 300 mm.")]
    [Display(Name = "Largura da lente (mm)")]
    public int? LensWidthMillimeters { get; set; }

    [Range(1, 300, ErrorMessage = "A ponte deve estar entre 1 e 300 mm.")]
    [Display(Name = "Largura da ponte (mm)")]
    public int? BridgeWidthMillimeters { get; set; }

    [Range(1, 300, ErrorMessage = "A haste deve estar entre 1 e 300 mm.")]
    [Display(Name = "Comprimento da haste (mm)")]
    public int? TempleLengthMillimeters { get; set; }

    [Display(Name = "Produto ativo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Publicado no catálogo")]
    public bool IsPublished { get; set; }

    public CreateFrameRequest ToCreateRequest() => new(
        Code,
        Brand,
        Model,
        Color,
        Price,
        StockQuantity,
        Type,
        Shape,
        TargetAudience,
        CreateMeasurements());

    public UpdateFrameRequest ToUpdateRequest() => new(
        Code,
        Brand,
        Model,
        Color,
        Price,
        StockQuantity,
        Type,
        Shape,
        TargetAudience,
        IsActive,
        IsPublished,
        CreateMeasurements());

    public static FrameInputModel FromCatalogItem(FrameCatalogItem frame) => new()
    {
        Code = frame.Code,
        Brand = frame.Brand,
        Model = frame.Model,
        Color = frame.Color,
        Price = frame.Price,
        StockQuantity = frame.StockQuantity,
        Type = frame.Type,
        Shape = frame.Shape,
        TargetAudience = frame.TargetAudience,
        LensWidthMillimeters = frame.LensWidthMillimeters,
        BridgeWidthMillimeters = frame.BridgeWidthMillimeters,
        TempleLengthMillimeters = frame.TempleLengthMillimeters,
        IsActive = frame.IsActive,
        IsPublished = frame.IsPublished
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var suppliedMeasurements = new[]
        {
            LensWidthMillimeters,
            BridgeWidthMillimeters,
            TempleLengthMillimeters
        }.Count(value => value.HasValue);

        if (suppliedMeasurements is > 0 and < 3)
        {
            yield return new ValidationResult(
                "Preencha as três medidas ou deixe todas em branco.",
                [nameof(LensWidthMillimeters), nameof(BridgeWidthMillimeters), nameof(TempleLengthMillimeters)]);
        }
    }

    private FrameMeasurements? CreateMeasurements()
    {
        if (!LensWidthMillimeters.HasValue)
        {
            return null;
        }

        return new FrameMeasurements(
            LensWidthMillimeters.Value,
            BridgeWidthMillimeters!.Value,
            TempleLengthMillimeters!.Value);
    }
}
