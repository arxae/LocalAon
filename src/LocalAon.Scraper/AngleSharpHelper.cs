using AngleSharp;

namespace LocalAon.Scraper;

internal static class AngleSharpHelper
{
    internal static IBrowsingContext GetBrowsingContext()
    {
        IConfiguration config = Configuration.Default;
        return BrowsingContext.New(config);
    }
}
