namespace LocalAon.Models;

public class DruidCompanion : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }

    public string? CompanionType { get; set; }
    public string? MonsterEntry { get; set; } // Url to the MonsterDisplay page
    public string? Description { get; set; }
}
