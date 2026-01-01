using LocalAon.Models;

namespace LocalAon.Scraper;

internal static class ModelExtensions
{
    extension(IPageModel model)
    {
        internal async Task AddToContextAsync(StorageContext dbContext)
        {
            await dbContext.AddAsync(model);
        }
    }
}
