namespace LocalAon.Models;

public class SpellDisplayItem : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Source { get; set; } = null!;

    // Page fields
    public string? School { get; set; }
    public string? Level { get; set; }
    public string? CastingTime { get; set; }
    public string? Components { get; set; }
    public string? Range { get; set; }
    public string? Target { get; set; }
    public string? Area { get; set; }
    public string? Duration { get; set; }
    public string? SavingThrow { get; set; }
    public string? SpellResistance { get; set; }
    public string? Description { get; set; }
}
