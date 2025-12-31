namespace LocalAon.Models;

public class TrapItem : IPageModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; }

    public string Name { get; set; }
    public string Source { get; set; }
    public string Type { get; set; }
    public string Perception { get; set; }
    public string DisableDevice { get; set; }
    public string Trigger { get; set; }
    public string Reset { get; set; }
    public string Effect { get; set; }
}
