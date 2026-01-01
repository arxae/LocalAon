using System.Collections.Concurrent;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models.Products;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Spectre.Console;

namespace LocalAon.Scraper.Scrapers;

/// <summary>
/// This will collect all the links from a product page (eg: https://www.aonprd.com/SourceDisplay.aspx?FixedSource=PRPG%20Core%20Rulebook)
/// Since this will be quite a high number (and we don't really need them afterwards), we will store them in a queue
/// for later processing.
/// </summary>
internal sealed class ProductPageScraper : IDisposable
{
    readonly StorageContext dbContext;
    readonly HttpClient client;
    readonly ILogger log =  Log.ForContext<ProductPageScraper>();

    internal ProductPageScraper(StorageContext dbContext)
    {
        this.dbContext = dbContext;
        client = new HttpClient();
    }

    internal async Task ScrapeAndSave()
    {
        List<Product> products = await dbContext.Products.ToListAsync();

        ConcurrentBag<ProductItem> allItems = [];
        ConcurrentBag<Product> retry = [];
        ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };
        int currentCount = 0;
        await Parallel.ForEachAsync(products, options, async (product, _) =>
        {
            try
            {
                List<ProductItem> items = await GetProductItemsFromProduct(product);
                items.ForEach(allItems.Add);

                // Every so often, wait for a little. Otherwise the website starts spitting 500 errors
                if (currentCount % Environment.ProcessorCount == 0)
                {
                    await Task.Delay(7500, _);
                }
            }
            catch (Exception)
            {
                retry.Add(product);
            }

            currentCount++;
        });

        if (retry.IsEmpty == false)
        {
            foreach (Product product in retry)
            {
                try
                {
                    List<ProductItem> queue = await GetProductItemsFromProduct(product);
                    foreach (ProductItem queueItem in queue)
                    {
                        allItems.Add(queueItem);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "An error occured while retrying {Product}", product);
                    retry.Add(product);
                }
            }
        }

        await dbContext.ProductedItems.ExecuteDeleteAsync();

        foreach (ProductItem item in allItems)
        {
            dbContext.ProductedItems.Add(item);
        }

        await dbContext.SaveChangesAsync();
    }

    internal async Task<List<ProductItem>> GetProductItemsFromProduct(Product product)
    {
        if (product.Url.StartsWith("https://www.aonprd.com/SourceDisplay.aspx?FixedSource=", StringComparison.OrdinalIgnoreCase) == false)
        {
            log.Error("Incorrect url for {Product}", product);
            return [];
        }

        string html = await client.GetStringAsync(product.Url);
        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        IDocument document = await context.OpenAsync(req => req.Content(html));
        IElement? root = document.QuerySelector("span#MainContent_FinalOutputLabel");

        if (root == null)
        {
            log.Error("Could't find root element for {Product}", product);
            return [];
        }

        List<ProductItem> productItems = [];
        string currentCategory = "Unknown";
        foreach (IElement child in root.Children)
        {
            // Track the category
            if (child is IHtmlHeadingElement h)
            {
                currentCategory = h.TextContent;

                // Omit the count from the category
                if (currentCategory.Contains('['))
                {
                    int index = currentCategory.IndexOf('[');
                    currentCategory = currentCategory[..index];
                }

                continue;
            }

            if (child is IHtmlAnchorElement a)
            {
                string url = Constants.FixLocalhostLink(a.Href);
                // Gets the website category. This is actually the aspx page used to display the item
                string websiteCategory = a.Href
                    .Replace(Constants.LOCALHOST, string.Empty)
                    .TrimStart('/')
                    .Split([".aspx"], 2, StringSplitOptions.None)[0];

                ProductItem item = new()
                {
                    ProductId = product.Id,
                    Url = url,
                    BookCategory = currentCategory,
                    WebsiteCategory = websiteCategory,
                    Processed = false,
                };

                productItems.Add(item);
            }
        }

        return productItems;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
