using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OticaVisao.Web.Pages;

public class IndexModel : PageModel
{
    public IReadOnlyList<FeaturedFrame> FeaturedFrames { get; } =
    [
        new("Ray-Ban", "RX5228", "Preto brilho", 599.00m, "black"),
        new("Armani Exchange", "AX3060", "Havana", 699.00m, "havana"),
        new("Vogue", "VO4240", "Dourado", 529.00m, "gold"),
        new("Oakley", "OX5138", "Preto fosco", 789.00m, "graphite")
    ];

    public void OnGet()
    {
    }
}

public sealed record FeaturedFrame(string Brand, string Model, string Color, decimal Price, string VisualStyle);
