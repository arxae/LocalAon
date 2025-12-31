namespace LocalAon.Scraper;

internal static class Constants
{
    internal const string AON_ROOT = "https://www.aonprd.com";
    internal const string LOCALHOST = "http://localhost";

    internal static string AssembleAonLink(string suffix)
    {
        bool includeDash = suffix.StartsWith("/", StringComparison.OrdinalIgnoreCase) == false;

        return AON_ROOT + (includeDash == true ? "/" : "")  + suffix;
    }

    internal static string FixLocalhostLink(string link)
        => link.Replace("http://localhost", AON_ROOT);
}
