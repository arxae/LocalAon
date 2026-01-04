namespace LocalAon.Models;

public class BloodlineDisplayItem : IPageModel
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }

    public string? Description { get; set; }
    public string? ClassSkill { get; set; }
    public string? BonusSpells { get; set; }
    public string? BonusFeats { get; set; }
    public string? BloodlineArcana { get; set; }
    public string? BloodlinePowers { get; set; }
}
