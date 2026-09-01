using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OticaVisao.Web.Pages;

public class IndexModel : PageModel
{
    public IReadOnlyList<FeaturedFrame> FeaturedFrames { get; } =
    [
        new("Modelo demonstrativo", "OV1001", "Preto brilho", 219.00m, "black"),
        new("Modelo demonstrativo", "OV1002", "Havana", 219.00m, "havana"),
        new("Modelo demonstrativo", "OV1003", "Dourado", 219.00m, "gold"),
        new("Modelo demonstrativo", "OV1004", "Preto fosco", 219.00m, "graphite")
    ];

    public void OnGet()
    {
    }
}

public sealed record FeaturedFrame(string Brand, string Model, string Color, decimal Price, string VisualStyle);
