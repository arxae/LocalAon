using System.Collections.Concurrent;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models;
using LocalAon.Models.Products;
using Serilog;
using Spectre.Console;

namespace LocalAon.Scraper.Scrapers.Pages;

/// <summary>
/// Extacts data from the SpellDisplay pages
/// eg: https://www.aonprd.com/SpellDisplay.aspx?ItemName=Permanency
/// </summary>
internal sealed class SpellDisplayScraper : IDisposable
{
    readonly StorageContext dbContext;
    readonly ProgressTask taskContext;
    readonly HttpClient client;
    readonly ILogger log =  Log.ForContext<SpellDisplayScraper>();

    internal SpellDisplayScraper(StorageContext dbContext, ProgressTask taskContext)
    {
        this.dbContext = dbContext;
        this.taskContext = taskContext;
        client = new HttpClient();
    }

    internal async Task ScrapeAndSave()
    {
        List<ProductItem> items = dbContext
            .ProductedItems
            .Where(p => p.WebsiteCategory == "SpellDisplay")
            .ToList();

        taskContext.MaxValue = items.Count;

        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        ConcurrentBag<SpellDisplayItem> results = [];
        await Parallel.ForEachAsync(items, options, async (spell, ct) =>
        {
            SpellDisplayItem? result = await ScrapeSpellPage(spell);

            if (result == null)
            {
                log.Error("Could not scrape Spell Page for {Url}", spell.Url);
            }
            else
            {
                results.Add(result);
                spell.Processed = true;
            }

            taskContext.Increment(1);
        });

        foreach (SpellDisplayItem r in results)
        {
            dbContext.Spells.Add(r);
        }

        await dbContext.SaveChangesAsync();
        taskContext.Description = $"Processed {items.Count} spells";
    }

    internal async Task<SpellDisplayItem?> ScrapeSpellPage(ProductItem productItem)
    {
        SpellDisplayItem spell = new() { Url = productItem.Url, ProductId = productItem.ProductId };

        string html = await client.GetStringAsync(productItem.Url);
        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        IDocument document = await context.OpenAsync(req => req.Content(html));
        IElement? root = document.QuerySelector("table#MainContent_DataListTypes");

        if (root == null)
            return null;

        IElement? titleNode = root.QuerySelector("h1.title");

        if (titleNode == null)
            return null;

        spell.Name = titleNode.TextContent.Trim();

        List<IElement> boldElements = [.. root.QuerySelectorAll("b")];

        spell.Source = boldElements
            .Where(b => string.Equals(b.TextContent, "Source", StringComparison.OrdinalIgnoreCase))
            .Select(b => b.NextElementSibling)
            .OfType<IHtmlAnchorElement>()
            .FirstOrDefault()!
            .TextContent;

        // Other page fields
        spell.School = NodeStringHelper.ExtractTextBetweenBoldLabels(document, "School", "Level");
        spell.Level = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Level");
        spell.CastingTime = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Casting Time");
        spell.Components = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Components");
        spell.Range = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Range");
        spell.Target = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Target");
        spell.Duration = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Duration");
        spell.Description = NodeStringHelper.ExtractDescriptionMarkdown(document);

        return spell;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
