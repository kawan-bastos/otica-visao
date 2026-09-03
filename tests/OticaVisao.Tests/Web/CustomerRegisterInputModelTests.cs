using System.ComponentModel.DataAnnotations;
using OticaVisao.Web.Models.Account;

namespace OticaVisao.Tests.Web;

public sealed class CustomerRegisterInputModelTests
{
    [Fact]
    public void CompleteRegistrationIsValid()
    {
        var model = ValidModel();

        Assert.Empty(Validate(model));
    }

    [Fact]
    public void RegistrationRejectsInvalidCpfAndFutureBirthDate()
    {
        var model = ValidModel();
        model.Cpf = "111.111.111-11";
        model.BirthDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        var results = Validate(model);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Cpf)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.BirthDate)));
    }

    private static CustomerRegisterInputModel ValidModel() => new()
    {
        DisplayName = "Maria Silva",
        Email = "maria@example.com",
        PhoneNumber = "(21) 99999-0000",
        Cpf = "529.982.247-25",
        BirthDate = new DateOnly(1990, 5, 10),
        PostalCode = "25931-770",
        Street = "Rua Arthur Rodrigues Loivos",
        Number = "370",
        Neighborhood = "Piabetá",
        City = "Magé",
        State = "RJ",
        Password = "Senha@123",
        ConfirmPassword = "Senha@123"
    };

    private static List<ValidationResult> Validate(CustomerRegisterInputModel model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }
}
