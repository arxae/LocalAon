namespace LocalAon.Models;

public class PoisonDisplay : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Source { get; set; } = null!;

    public string? Price { get; set; }
    public string? Weight { get; set; }
    public string? Type { get; set; }
    public string? Save { get; set; }
    public string? Onset { get; set; }
    public string? Frequeny { get; set; }
    public string? Effect { get; set; }
    public string? Cure { get; set; }
    public string? Description { get; set; }

    public const string WEBSITE_CATEGORY = nameof(PoisonDisplay);
}
