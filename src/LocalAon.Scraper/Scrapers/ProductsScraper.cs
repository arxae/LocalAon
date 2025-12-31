using System.Globalization;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models.Products;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace LocalAon.Scraper.Scrapers;

/// <summary>
/// This class will scrape the contents of the sources page (https://www.aonprd.com/Sources.aspx)
/// </summary>
internal sealed class ProductsScraper : IDisposable
{
    // The different product lines at the top of the page
    readonly string[] productLines =
    [
        "AdventurePath",
        "CampaignSetting",
        "Module",
        "PlayerCompanion",
        "RPG",
        "Miscellaneous"
    ];

    readonly StorageContext dbContext;
    readonly HttpClient client;
    readonly ProgressTask progressContext;

    internal ProductsScraper(StorageContext storageContext, ProgressTask ctx)
    {
        dbContext  = storageContext;

        client = new HttpClient();
        client.BaseAddress = new Uri(Constants.AON_ROOT);

        progressContext = ctx;
        progressContext.IsIndeterminate = true;
    }

    internal async Task ScrapeAndSave()
    {
        List<Product> prods = await ScrapeProducts();

        progressContext.Description = $"Retrieved {prods.Count} products";

        foreach (Product product in prods)
        {
            if (await dbContext.Products.AnyAsync(p => p.Url == product.Url))
            {
                // TODO: Log to file/db
                //log.Warning("Product {Name} already saved", product.ProductName);
                continue;
            }

            await dbContext.Products.AddAsync(product);
        }

        await dbContext.SaveChangesAsync();
        progressContext.Value = 100;
        progressContext.Description = $"Retrieved {prods.Count} products";
    }

    async Task<List<Product>> ScrapeProducts()
    {
        List<Product> output = [];

        IBrowsingContext context = AngleSharpHelper.GetBrowsingContext();
        foreach (string productLine in productLines)
        {
            string productUrl = $"Sources.aspx?ProductLine={productLine}";

            progressContext.Description = $"Scanning {productUrl}";

            string html = await client.GetStringAsync(productUrl);
            IDocument document = await context.OpenAsync(req => req.Content(html));

            document.QuerySelectorAll("#MainContent_GridViewSources tr:not(:first-child)")
                .Select(row =>
                {
                    IHtmlCollection<IElement> cols = row.QuerySelectorAll("td");
                    IHtmlAnchorElement a = (IHtmlAnchorElement)cols[0].QuerySelector("a")!;

                    string name = a.TextContent;
                    string url = a.Href.Replace("http://localhost", Constants.AON_ROOT);
                    string dateString = cols[1].TextContent;
                    string releaseDate = DateTime
                        .ParseExact(dateString, "M/d/yyyy", CultureInfo.InvariantCulture)
                        .ToString("u");

                    return new Product(productLine, name, url, releaseDate);
                })
                .ToList()
                .ForEach(output.Add);
        }

        return output;
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
