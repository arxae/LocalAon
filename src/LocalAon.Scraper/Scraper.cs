using System.Collections.Concurrent;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models;
using LocalAon.Models.Products;
using Serilog;

namespace LocalAon.Scraper;

internal class Scraper<TModel> : IDisposable
    where TModel : class, IPageModel, new()
{
    readonly StorageContext dbContext;
    readonly HttpClient client;

    readonly ILogger log = Log.ForContext<Scraper<TModel>>();

    internal required string WebsiteCategory;
    internal required string RootElementSelector;
    internal required string NameSelector;
    internal required Action<TModel, IDocument, IElement> PopulateModel;

    internal Scraper(StorageContext dbContext)
    {
        this.dbContext = dbContext;
        client = new HttpClient();
    }

    internal async Task ScrapeAndSave()
    {
        List<ProductItem> items = dbContext
            .ProductedItems
            .Where(p => p.WebsiteCategory == WebsiteCategory)
            .ToList();

        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        ConcurrentBag<TModel> results = [];
        await Parallel.ForEachAsync(items, options, async (item, ct) =>
        {
            TModel? result = await Scrape(item);

            if (result == null)
            {
                log.Error("Could not scrape page for {Url}", item.Url);
            }
            else
            {
                results.Add(result);
                item.Processed = true;
            }
        });

        dbContext.Set<TModel>().AddRange(results);

        await dbContext.SaveChangesAsync();
    }

    internal async Task<TModel?> Scrape(ProductItem productItem)
    {
        TModel item = new() { Url = productItem.Url, ProductId = productItem.ProductId };

        string html = await client.GetStringAsync(productItem.Url);
        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        IDocument document = await context.OpenAsync(req => req.Content(html));
        IElement? root = document.QuerySelector(RootElementSelector);

        if (root == null)
            return null;

        IElement? titleNode = root.QuerySelector(NameSelector);

        if (titleNode == null)
            return null;

        item.Name = titleNode.TextContent.Trim();

        List<IElement> boldElements = [.. root.QuerySelectorAll("b")];

        item.Source = boldElements
            .Where(b => string.Equals(b.TextContent, "Source", StringComparison.OrdinalIgnoreCase))
            .Select(b => b.NextElementSibling)
            .OfType<IHtmlAnchorElement>()
            .FirstOrDefault()!
            .TextContent;

        PopulateModel(item, document, root);

        return item;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
