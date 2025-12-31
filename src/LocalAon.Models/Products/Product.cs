namespace LocalAon.Models.Products;

/// <summary>
/// A product represents a book or other media that contains contents
/// Any one item from this page: https://www.aonprd.com/Sources.aspx
/// </summary>
public class Product
{
    public int Id { get; init; }

    /// <summary>
    /// Eg Adventure Path, Campaign Setting, etc..
    /// </summary>
    public string ProductLine { get; init; }

    /// <summary>
    /// The name of the media. Eg: PRPG Core Rulebook
    /// </summary>
    public string ProductName { get; init; }

    /// <summary>
    /// The url to the product index with all the content
    /// Eg: https://www.aonprd.com/SourceDisplay.aspx?FixedSource=PRPG%20Core%20Rulebook
    /// </summary>
    public string Url { get; init; }

    /// <summary>
    /// The date the product was released
    /// </summary>
    public string ReleaseDate { get; init; }

    public Product(string productLine, string productName, string url, string releaseDate)
    {
        ProductLine = productLine;
        ProductName = productName;
        Url = url;
        ReleaseDate = releaseDate;
    }

    public override string ToString() => $"{ProductLine} - {ProductName} - {Url} - {ReleaseDate}";
}
