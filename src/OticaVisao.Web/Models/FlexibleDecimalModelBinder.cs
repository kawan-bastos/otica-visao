using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OticaVisao.Web.Models;

public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var context = bindingContext;
        var result = context.ValueProvider.GetValue(context.ModelName);
        if (result == ValueProviderResult.None) return Task.CompletedTask;
        context.ModelState.SetModelValue(context.ModelName, result);

        var text = result.FirstValue?.Trim();
        if (string.IsNullOrEmpty(text) && Nullable.GetUnderlyingType(context.ModelType) is not null)
        {
            context.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        // Com vírgula, segue o padrão brasileiro e remove pontos de milhar.
        // Sem vírgula, o ponto é tratado como separador decimal (ex.: 1.75).
        var normalized = text?.Contains(',') == true
            ? text.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.')
            : text;
        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            context.Result = ModelBindingResult.Success(value);
        else
            context.ModelState.TryAddModelError(context.ModelName, "Informe um número válido.");

        return Task.CompletedTask;
    }
}

public sealed class FlexibleDecimalModelBinderProvider : IModelBinderProvider
{
    private static readonly IModelBinder Binder = new FlexibleDecimalModelBinder();
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Metadata.ModelType) ?? context.Metadata.ModelType;
        return type == typeof(decimal) ? Binder : null;
    }
}
