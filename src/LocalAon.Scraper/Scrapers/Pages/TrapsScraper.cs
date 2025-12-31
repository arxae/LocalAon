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
/// Extracts data from the Traps pages
/// https://www.aonprd.com/Traps.aspx?ItemName=Wyvern%20Arrow%20Trap
/// </summary>
internal sealed class TrapsScraper : IDisposable
{
    readonly StorageContext dbContext;
    readonly ProgressTask taskContext;
    readonly HttpClient client;
    readonly ILogger log =  Log.ForContext<TrapsScraper>();

    internal TrapsScraper(StorageContext dbContext, ProgressTask taskContext)
    {
        this.dbContext = dbContext;
        this.taskContext = taskContext;
        client = new HttpClient();
    }

    internal async Task ScrapeAndSave()
    {
        List<ProductItem> items = dbContext
            .ProductedItems
            .Where(p => p.WebsiteCategory == "Traps")
            .ToList();

        taskContext.MaxValue = items.Count;

        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        ConcurrentBag<TrapItem> results = [];
        await Parallel.ForEachAsync(items, options, async (spell, ct) =>
        {
            TrapItem? result = await ScrapeTrapPage(spell);

            if (result == null)
            {
                log.Error("Could not scrape Trap Page for {Url}", spell.Url);
            }
            else
            {
                results.Add(result);
                spell.Processed = true;
            }

            taskContext.Increment(1);
        });

        foreach (TrapItem r in results)
        {
            dbContext.Traps.Add(r);
        }

        await dbContext.SaveChangesAsync();
        taskContext.Description = $"Processed {items.Count} traps";
    }

    internal async Task<TrapItem?> ScrapeTrapPage(ProductItem productItem)
    {
        TrapItem trap = new() { Url = productItem.Url, ProductId = productItem.ProductId };

        string html = await client.GetStringAsync(productItem.Url);
        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        IDocument document = await context.OpenAsync(req => req.Content(html));
        IElement? root = document.QuerySelector("#MainContent_DataListTraps");

        if (root == null)
            return null;

        IElement? titleNode = root.QuerySelector("h2.title > a");

        if (titleNode == null)
            return null;

        trap.Name = titleNode.TextContent.Trim();

        List<IElement> boldElements = [.. root.QuerySelectorAll("b")];

        trap.Source = boldElements
            .Where(b => string.Equals(b.TextContent, "Source", StringComparison.OrdinalIgnoreCase))
            .Select(b => b.NextElementSibling)
            .OfType<IHtmlAnchorElement>()
            .FirstOrDefault()!
            .TextContent;

        // Other page fields
        trap.Type = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Type");
        trap.Perception = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Perception");
        trap.DisableDevice = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Disable Device");
        trap.Trigger = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Trigger");
        trap.Reset = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Reset");
        trap.Effect = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Effect");

        return trap;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
