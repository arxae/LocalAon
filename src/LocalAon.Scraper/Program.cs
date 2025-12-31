using LocalAon.Models;
using LocalAon.Models.Products;
using Serilog;
using Spectre.Console;
using LocalAon.Scraper.Scrapers;
using LocalAon.Scraper.Scrapers.Pages;
using Microsoft.EntityFrameworkCore;

namespace LocalAon.Scraper;

public static class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.File("output.log",
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u3}][{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        StorageContext dbContext = new();
        await dbContext.Database.EnsureCreatedAsync();

        ProductItem testItem = dbContext.ProductedItems.FirstOrDefault(p => p.WebsiteCategory == "Traps")!;

        Scraper<TrapItem> trapScraper = new(dbContext)
        {
            WebsiteCategory = "Traps",
            RootElementSelector = "#MainContent_DataListTraps",
            NameSelector = "h2.title > a",
            PopulateModel = (trap, document, root) =>
            {
                trap.Type = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Type");
                trap.Perception = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Perception");
                trap.DisableDevice = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Disable Device");
                trap.Trigger = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Trigger");
                trap.Reset = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Reset");
                trap.Effect = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Effect");
            }
        };

        TrapItem? result = await trapScraper.Scrape(testItem);



        return;

        await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
                // Get all the products
                ProgressTask productsTask = ctx.AddTask("Getting all products");
                ProductsScraper productsScraper = new(dbContext, productsTask);
                await productsScraper.ScrapeAndSave();
                productsScraper.Dispose();

#if DEBUG
                // Keep only the core rule book for testing
                // Log.Warning("Running in debug, deleting all records except Core.");
                await dbContext.Products
                    .Where(e => e.Id != 454)
                    .ExecuteDeleteAsync();

                await dbContext.SaveChangesAsync();

                ProgressTask debugPurgeTask = ctx.AddTask(Markup.Escape("[DEBUG] Remove everything but core product"));
                debugPurgeTask.Value = 100;
#endif

                // Get all the items from the products page
                ProgressTask productsItemTask = ctx.AddTask("Getting product items");
                ProductPageScraper productPageScraper = new(dbContext, productsItemTask);
                await productPageScraper.ScrapeAndSave();
                productPageScraper.Dispose();

                // We now start actually scraping all the pages. Add all the progress bars first
                ProgressTask spellTask = ctx.AddTask("Getting spells");
                ProgressTask trapsTask = ctx.AddTask("Getting traps");

                // Scrape Spells
                // SpellDisplayScraper sds = new(dbContext, spellTask);
                // await sds.ScrapeAndSave();
                // sds.Dispose();

                // Scrape Traps
                TrapsScraper trapsScraper = new(dbContext, trapsTask);
                await trapsScraper.ScrapeAndSave();
                trapsScraper.Dispose();
            });
    }
}
