namespace LocalAon.Models;

public class SpellDisplayItem : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int ProductId { get; set; }

    // Page fields
    public string Name { get; set; }
    public string Source { get; set; }
    public string School { get; set; }
    public string Level { get; set; }
    public string CastingTime { get; set; }
    public string Components { get; set; }
    public string Range { get; set; }
    public string Target { get; set; }
    public string Duration { get; set; }
    public string Description { get; set; }
}
