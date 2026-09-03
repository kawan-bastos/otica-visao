using System.ComponentModel.DataAnnotations;
using OticaVisao.Domain.Sales;
using OticaVisao.Web.Models.Admin;

namespace OticaVisao.Tests.Web;

public sealed class SaleItemInputModelTests
{
    [Fact]
    public void CompleteGlassesRequiresLensDescriptionAndLaboratory()
    {
        var model = new SaleItemInputModel { FrameId = Guid.NewGuid(), IncludesLenses = true };

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.LensDescription)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Laboratory)));
    }

    [Fact]
    public void FrameOnlyIgnoresLensValuesInRequest()
    {
        var model = new SaleItemInputModel
        {
            FrameId = Guid.NewGuid(),
            IncludesLenses = false,
            LensDescription = "valor antigo",
            LensUnitPrice = 300m,
            Laboratory = OpticalLaboratory.ImperialLab
        };

        var request = model.ToRequest();

        Assert.Null(request.LensDescription);
        Assert.Equal(0, request.LensUnitPrice);
        Assert.Null(request.Laboratory);
    }

    private static List<ValidationResult> Validate(SaleItemInputModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, true);
        return results;
    }
}
