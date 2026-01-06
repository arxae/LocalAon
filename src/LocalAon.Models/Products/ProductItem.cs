namespace LocalAon.Models.Products;

/// <summary>
/// This class represent a link to a content item on the product page (eg: https://www.aonprd.com/SourceDisplay.aspx?FixedSource=PRPG%20Core%20Rulebook)
/// </summary>
public class ProductItem
{
    public int Id { get; init; }

    /// <summary>
    /// The id of the product this content is associated with
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// The url towards the specific content
    /// eg: https://www.aonprd.com/Curses.aspx?ItemName=Baleful%20Polymorph%20Spell
    /// </summary>
    public string Url { get; init; } = null!;

    /// <summary>
    /// The full category used on the sources page
    /// eg: Afflictions - Curses
    /// </summary>
    public string BookCategory { get; init; } = null!;

    /// <summary>
    /// The category used on the website. This is actually the aspx page used to display the item
    /// eg: https://www.aonprd.com/Curses.aspx?ItemName=Baleful%20Polymorph%20Spell -> Curses
    /// </summary>
    public string WebsiteCategory { get; init; } = null!;

    /// <summary>
    /// Whether this item was scraped or not.
    /// </summary>
    public bool Processed { get; set; }
}
