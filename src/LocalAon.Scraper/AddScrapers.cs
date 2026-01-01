using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models;
using Serilog;

namespace LocalAon.Scraper;

internal static class AddScrapers
{
    static ILogger GetLogger(string ctx)
        => Log.ForContext("SourceContext", ctx);

    internal static Dictionary<string, IScraper> Get(StorageContext dbContext)
    {
        return new Dictionary<string, IScraper>()
        {
            ["Curses"] = new Scraper<Curse>(dbContext)
            {
                WebsiteCategory = "Curses",
                RootElementSelector = "table#MainContent_DataListCurses",
                NameSelector = "h1.title",
                PopulateModel = (curse, document, root) =>
                {
                    curse.Type = NodeStringHelper.ExtractTextBetweenBoldLabels(document, "Type", "Save");
                    curse.Save = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Save");
                    curse.Onset = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Onset");
                    curse.Effect = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Effect");
                    curse.Frequency = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Frequency");
                }
            },
            ["Diseases"] = new Scraper<Disease>(dbContext)
            {
                WebsiteCategory = "Curses",
                RootElementSelector = "table#MainContent_DataListDiseases",
                NameSelector = "h1.title",
                PopulateModel = (disease, document, root) =>
                {
                    disease.Type = NodeStringHelper.ExtractTextBetweenBoldLabels(document, "Type", "Fortitude Save");
                    disease.FortitudeSave = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Fortitude Save");
                    disease.Onset = NodeStringHelper.ExtractTextBetweenBoldLabels(document, "Onset", "Frequency");
                    disease.Frequency = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Frequency");
                    disease.Effect = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Effect");
                    disease.Cure = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Frequency");
                }
            },
            ["DruidCompanions"] = new Scraper<DruidCompanion>(dbContext)
            {
                WebsiteCategory = "DruidCompanions",
                RootElementSelector = "span#MainContent_DetailedOutput",
                NameSelector = "h1.title",
                PopulateModel = (druidCompanion, document, root) =>
                {
                    // Get the monster link node
                    INode? monsterEntryNode = root.QuerySelectorAll("i")
                        .FirstOrDefault(e => e.TextContent == "Link")!
                        .Parent;

                    string? monsterEntryLink = (monsterEntryNode as IHtmlAnchorElement)?.Href;
                    if (monsterEntryLink != null)
                    {
                        monsterEntryLink = Constants.FixLocalhostLink(monsterEntryLink);
                    }

                    string monsterEntryMd = NodeStringHelper.ExtractAllAfterTag(document, monsterEntryNode);

                    druidCompanion.CompanionType = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Companion Type");
                    druidCompanion.MonsterEntry = monsterEntryLink;
                    druidCompanion.Description = monsterEntryMd;
                }
            },
            ["SpellDisplay"] = new Scraper<SpellDisplayItem>(dbContext)
            {
                WebsiteCategory = "SpellDisplay",
                RootElementSelector = "table#MainContent_DataListTypes",
                NameSelector = "h1.title",
                PopulateModel = (spell, document, root) =>
                {
                    spell.School = NodeStringHelper.ExtractTextBetweenBoldLabels(document, "School", "Level");
                    spell.Level = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Level");
                    spell.CastingTime = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Casting Time");
                    spell.Components = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Components");
                    spell.Range = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Range");
                    spell.Target = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Target");
                    spell.Duration = NodeStringHelper.ExtractNextTextAfterBoldLabel(document, "Duration");
                    spell.Description = NodeStringHelper.ExtractDescriptionMarkdown(document);
                }
            },
            ["Traps"] = new Scraper<TrapItem>(dbContext)
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
            }
        };
    }
}
