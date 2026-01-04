using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using LocalAon.Models;
using Serilog;

namespace LocalAon.Scraper;

internal static class ScraperSetup
{
    static void Error(string ctx, string template, params object?[]? templateObj)
        => Log.ForContext("SourceContext", ctx).Error(template, templateObj);

    internal static Dictionary<string, IScraper> Get(StorageContext dbContext)
    {
        return new Dictionary<string, IScraper>()
        {
            ["BloodlineDisplay"] = new Scraper<BloodlineDisplayItem>(dbContext)
            {
                WebsiteCategory = "BloodlineDisplay",
                RootElementSelector = "table#MainContent_DataListTypes",
                NameSelector = "h1.title",
                PopulateModel = (bloodline, document, root) =>
                {
                    IElement? sourceNode = NodeHelper.GetSourceNode(root);

                    if (sourceNode == null)
                    {
                        Error("Scraper::BloodlineDisplay", "Unable to find 'Source' node. This might indicate the page layout has changed");
                    }

                    bloodline.Description = sourceNode
                        ?.NextSibling
                        ?.NextSibling
                        ?.TextContent;

                    bloodline.ClassSkill = NodeHelper.GetTextAfterBoldNode(document, "Class Skill");
                    bloodline.BonusSpells = NodeHelper.GetTextAfterBoldNode(document, "Bonus Spells", true);
                    bloodline.BonusFeats = NodeHelper.GetTextAfterBoldNode(document, "Bonus Feats");
                    bloodline.BloodlineArcana = NodeHelper.GetTextAfterBoldNode(document, "Bloodline Arcana");

                    INode? bloodlinePowersNode = root.QuerySelectorAll("b")
                        .FirstOrDefault(e => e.TextContent == "Bloodline Powers");

                    bloodline.BloodlinePowers = NodeHelper.GetAllTextAfterNode(bloodlinePowersNode);
                }
            },
            ["Curses"] = new Scraper<Curse>(dbContext)
            {
                WebsiteCategory = "Curses",
                RootElementSelector = "table#MainContent_DataListCurses",
                NameSelector = "h1.title",
                PopulateModel = (curse, document, _) =>
                {
                    curse.Type = NodeHelper.GetTextAfterBoldNode(document, "Type");
                    curse.Save = NodeHelper.GetTextAfterBoldNode(document, "Save");
                    curse.Onset = NodeHelper.GetTextAfterBoldNode(document, "Onset");
                    curse.Effect = NodeHelper.GetTextAfterBoldNode(document, "Effect");
                    curse.Frequency = NodeHelper.GetTextAfterBoldNode(document, "Frequency");
                }
            },
            ["Diseases"] = new Scraper<Disease>(dbContext)
            {
                WebsiteCategory = "Curses",
                RootElementSelector = "table#MainContent_DataListDiseases",
                NameSelector = "h1.title",
                PopulateModel = (disease, document, _) =>
                {
                    disease.Type = NodeHelper.GetTextAfterBoldNode(document, "Type");
                    disease.FortitudeSave = NodeHelper.GetTextAfterBoldNode(document, "Fortitude Save");
                    disease.Onset = NodeHelper.GetTextAfterBoldNode(document, "Onset");
                    disease.Frequency = NodeHelper.GetTextAfterBoldNode(document, "Frequency");
                    disease.Effect = NodeHelper.GetTextAfterBoldNode(document, "Effect");
                    disease.Cure = NodeHelper.GetTextAfterBoldNode(document, "Frequency");
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

                    druidCompanion.CompanionType = NodeHelper.GetTextAfterBoldNode(document, "Companion Type");
                    druidCompanion.MonsterEntry = monsterEntryLink;

                    string monsterEntryHtml = NodeHelper.GetAllTextAfterNode(monsterEntryNode, true);
                    druidCompanion.Description = NodeHelper.ConvertToMarkdown(monsterEntryHtml);
                }
            },
            ["SpellDisplay"] = new Scraper<SpellDisplayItem>(dbContext)
            {
                WebsiteCategory = "SpellDisplay",
                RootElementSelector = "table#MainContent_DataListTypes",
                NameSelector = "h1.title",
                PopulateModel = (spell, document, root) =>
                {
                    spell.School = NodeHelper.GetTextAfterBoldNode(document, "School", true);
                    spell.Level = NodeHelper.GetTextAfterBoldNode(document, "Level");
                    spell.CastingTime = NodeHelper.GetTextAfterBoldNode(document, "Casting Time");
                    spell.Components = NodeHelper.GetTextAfterBoldNode(document, "Components");
                    spell.Range = NodeHelper.GetTextAfterBoldNode(document, "Range");
                    spell.Target = NodeHelper.GetTextAfterBoldNode(document, "Target");
                    spell.Area = NodeHelper.GetTextAfterBoldNode(document, "Area");
                    spell.Duration = NodeHelper.GetTextAfterBoldNode(document, "Duration");
                    spell.SavingThrow = NodeHelper.GetTextAfterBoldNode(document, "Saving Throw");
                    spell.SpellResistance = NodeHelper.GetTextAfterBoldNode(document, "Spell Resistance");

                    IElement? descriptionNode = root.QuerySelectorAll("h3.framing")
                        .FirstOrDefault(el => el.TextContent == "Description");
                    spell.Description = NodeHelper.GetAllTextAfterNode(descriptionNode);
                }
            },
            ["Traps"] = new Scraper<TrapItem>(dbContext)
            {
                WebsiteCategory = "Traps",
                RootElementSelector = "#MainContent_DataListTraps",
                NameSelector = "h2.title > a",
                PopulateModel = (trap, document, _) =>
                {
                    trap.Type = NodeHelper.GetTextAfterBoldNode(document, "Type");
                    trap.Perception = NodeHelper.GetTextAfterBoldNode(document, "Perception");
                    trap.DisableDevice = NodeHelper.GetTextAfterBoldNode(document, "Disable Device");
                    trap.Trigger = NodeHelper.GetTextAfterBoldNode(document, "Trigger");
                    trap.Reset = NodeHelper.GetTextAfterBoldNode(document, "Reset");
                    trap.Effect = NodeHelper.GetTextAfterBoldNode(document, "Effect");
                }
            }
        };
    }
}
