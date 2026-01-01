namespace LocalAon.Models;

public class Curse : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }

    public string? Type { get; set; }
    public string? Save { get; set; }
    public string? Onset { get; set; }
    public string? Effect { get; set; }
    public string? Frequency { get; set; }
}
