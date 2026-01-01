namespace LocalAon.Models;

public class Disease : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; }

    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }

    public string? Type { get; set; }
    public string? FortitudeSave { get; set; }
    public string? Onset { get; set; }
    public string? Frequency { get; set; }
    public string? Effect { get; set; }
    public string? Cure { get; set; }
}
