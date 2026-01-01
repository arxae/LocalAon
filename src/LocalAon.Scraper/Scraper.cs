using AngleSharp;
using AngleSharp.Dom;
using LocalAon.Models;
using LocalAon.Models.Products;
using Serilog;

namespace LocalAon.Scraper;

internal interface IScraper
{
    Task<IPageModel?> Scrape(ProductItem productItem);
}

internal class Scraper<TModel> : IScraper, IDisposable
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

    public async Task<IPageModel?> Scrape(ProductItem productItem)
    {
        TModel item = new() { Url = productItem.Url, ProductId = productItem.ProductId };

        string html = await client.GetStringAsync(productItem.Url);
        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        IDocument document = await context.OpenAsync(req => req.Content(html));
        IElement? root = document.QuerySelector(RootElementSelector);

        if (root == null)
        {
            log.Error("Root element not found for {Url}", productItem.Url);
            return null;
        }

        IElement? titleNode = root.QuerySelector(NameSelector);

        if (titleNode == null)
        {
            log.Error("Name element not found for {Url}", productItem.Url);
            return null;
        }

        item.Name = titleNode.TextContent.Trim();

        List<IElement> boldElements = [.. root.QuerySelectorAll("b")];

        item.Source = root.QuerySelector("a.external-link > i")!.TextContent;

        PopulateModel(item, document, root);

        return item;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
