using System.Threading.Channels;
using LocalAon.Models;
using LocalAon.Models.Products;
using LocalAon.Scraper.Scrapers;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace LocalAon.Scraper;

public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        StorageContext dbContext = new();
        await dbContext.Database.EnsureCreatedAsync();

        // Get all the products
        Log.Information("Getting all products");
        ProductsScraper productsScraper = new(dbContext);
        await productsScraper.ScrapeAndSave();
        productsScraper.Dispose();

#if DEBUG
        // Keep only the core rule book for testing
        Log.Warning("Running in debug, deleting all records except Core.");
        await dbContext.Products
            .Where(e => e.Id != 454)
            .ExecuteDeleteAsync();

        await dbContext.SaveChangesAsync();
#endif

        // Get all the items from the products page
        Log.Information("Getting all product items");
        ProductPageScraper productPageScraper = new(dbContext);
        await productPageScraper.ScrapeAndSave();
        productPageScraper.Dispose();

        using CancellationTokenSource cts = new();

        // Setup the page scrapers
        Dictionary<string, IScraper> scrapers = ScraperSetup.Get(dbContext, false);

        // Get all the product items that need to be processed
        Log.Information("Retrieving all product items");
        List<ProductItem> items = await dbContext.ProductItems
            .Where(pi => // Debug, only select specific categories to speed things up a little bit
                pi.WebsiteCategory == "BloodlineDisplay" ||
                pi.WebsiteCategory == "Curses" ||
                pi.WebsiteCategory == "Diseases" ||
                pi.WebsiteCategory == "DruidCompanions" ||
                pi.WebsiteCategory == "SpellDisplay" ||
                pi.WebsiteCategory == "Traps"
            )
            .Where(pi => pi.Processed == false)
            .ToListAsync();

        Log.Information("Retrieved {Count} product items", items.Count);

        Channel<ProductItem> channel = Channel.CreateBounded<ProductItem>(new BoundedChannelOptions(5)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        Task producer = Task.Run(async () =>
        {
            foreach (ProductItem item in items)
            {
                await channel.Writer.WriteAsync(item, cts.Token);
            }

            channel.Writer.Complete();
        }, cts.Token);

        int maxTasks = 5;
        int counter = 0;
        Task[] consumers = Enumerable.Range(0, maxTasks)
            .Select(_ => Task.Run(async () =>
            {
                await using StorageContext dbCtx = new();
                await foreach (ProductItem item in channel.Reader.ReadAllAsync(cts.Token))
                {
                    int counterInt = Interlocked.Increment(ref counter);

                    IPageModel? result = await scrapers[item.WebsiteCategory].Scrape(item);

                    if (result == null)
                    {
                        Log.Error("Unable to scrape {Item}", item.Url);
                        continue;
                    }

                    Log.Information("Processed {Name} - {Category} ({Curr}/{Total})",
                        result.Name, item.WebsiteCategory, counterInt, items.Count);

                    await result.AddToContextAsync(dbCtx);
                    await dbCtx.SaveChangesAsync(cts.Token);

                    // Wait a little bit to avoid 500 errors
                    await Task.Delay(500, cts.Token);
                }
            }, cts.Token))
            .ToArray();

        await Task.WhenAll(consumers.Append(producer));
    }
}
