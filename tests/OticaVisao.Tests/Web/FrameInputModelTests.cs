using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.Catalog;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Tests.Web;

public sealed class FrameInputModelTests
{
    [Fact]
    public void PartialMeasurementsAreRejected()
    {
        var input = ValidInput();
        input.LensWidthMillimeters = 52;

        var validationResults = Validate(input);

        Assert.Contains(validationResults, result =>
            result.ErrorMessage == "Preencha as três medidas ou deixe todas em branco.");
    }

    [Fact]
    public void UndefinedCatalogOptionIsRejected()
    {
        var input = ValidInput();
        input.Type = (FrameType)999;

        var validationResults = Validate(input);

        Assert.Contains(validationResults, result =>
            result.MemberNames.Contains(nameof(FrameInputModel.Type)));
    }

    private static List<ValidationResult> Validate(FrameInputModel input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }

    private static FrameInputModel ValidInput() => new()
    {
        Code = "ARM-100",
        Brand = "Linha Visão",
        Model = "Clássica",
        Color = "Preto",
        Price = 219m,
        StockQuantity = 1,
        Type = FrameType.Prescription,
        Shape = FrameShape.Square,
        TargetAudience = TargetAudience.Adult
    };
}
