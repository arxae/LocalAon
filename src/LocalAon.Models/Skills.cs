namespace LocalAon.Models;

public class Skills : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; } = null!;
    public int ProductId { get; set; }
    public string Name { get; set; } = null!;
    public string Source { get; set; } = null!;

    public string? Ability { get; set; }
    public bool ArmorCheckPenalty { get; set; }
    public bool TrainedOnly { get; set; }
    public string? Description { get; set; }

    public const string WEBSITE_CATEGORY = nameof(Skills);
}
