using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using ReverseMarkdown;

namespace LocalAon.Scraper;

internal static class NodeStringHelper
{
    /// <summary>
    /// Extracts the text between 2 bold tags.
    /// For example: School universal; Level arcanist 5, psychic 5, sorcerer 5, wizard 5
    /// School and Level are bold tags. Using these as from and to will return 'universal'.
    ///
    /// This will also catch if the tags are broken up. This happens often with School where it could
    /// show 'enchantment (compulsion) [mind-affecting];' for example. These tags will be reconstructed as a string
    /// </summary>
    /// <param name="document"></param>
    /// <param name="fromLabel"></param>
    /// <param name="toLabel"></param>
    /// <returns></returns>
    internal static string ExtractTextBetweenBoldLabels(IDocument document, string fromLabel, string toLabel)
    {
        IElement? start = document.QuerySelectorAll("b")
            .FirstOrDefault(b => b.TextContent.Trim()
                .Equals(fromLabel, StringComparison.OrdinalIgnoreCase));

        if (start == null)
            return string.Empty;

        StringBuilder sb = new();

        for (INode? node = start.NextSibling; node != null; node = node.NextSibling)
        {
            if (node is IElement { LocalName: "b" } el &&
                el.TextContent.Trim().Equals(toLabel, StringComparison.OrdinalIgnoreCase))
                break;

            CollectText(node, sb);
        }

        return Normalize(sb.ToString());
    }

    static void CollectText(INode node, StringBuilder sb)
    {
        if (node is IText text)
        {
            sb.Append(text.Data);
            return;
        }

        if (node is IElement element)
        {
            foreach (INode child in element.ChildNodes)
                CollectText(child, sb);
        }
    }

    static string Normalize(string text)
    {
        text = text
            .Replace("\u00A0", " ")
            .Replace("\r", "")
            .Replace("\n", "")
            .Replace("\t", " ");

        text = Regex.Replace(text, @"\s+", " ");
        text = Regex.Replace(text, @"\(\s*", "(");
        text = Regex.Replace(text, @"\s*\)", ")");
        text = Regex.Replace(text, @"\[\s*", "[");
        text = Regex.Replace(text, @"\s*\]", "]");

        if (text.EndsWith(':'))
        {
            text = text.Substring(0, text.Length - 1);
        }

        return text.Trim();
    }

    /// <summary>
    /// Extracts all the text after a bold label, but has no next label to search in between.
    /// Level on the spell pages, for example. The next tag is a h3
    /// </summary>
    /// <param name="document"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    internal static string ExtractNextTextAfterBoldLabel(IDocument document, string label)
    {
        IElement? start = document.All
            .OfType<IElement>()
            .FirstOrDefault(e =>
                (e.LocalName == "b") && e.TextContent.Trim()
                    .Equals(label, StringComparison.OrdinalIgnoreCase));

        return start == null
            ? string.Empty
            : ExtractNextTextNode(start);
    }

    internal static string ExtractNextTextNode(INode node)
    {
        for (INode? n = node.NextSibling; n != null; n = n.NextSibling)
        {
            if (n is IText text && !string.IsNullOrWhiteSpace(text.Data))
                return Normalize(text.Data);
        }

        return string.Empty;
    }

    /// <summary>
    /// This extracts the description section as pure html. Should be converted to markdown
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    public static string ExtractDescriptionMarkdown(IDocument document)
    {
        // Find the start node
        IElement? start = document.QuerySelectorAll("h3.framing")
            .FirstOrDefault(el => el.TextContent == "Description");

        if (start == null)
            return string.Empty;

        StringBuilder sb = new();

        // Walk through following siblings
        for (INode? node = start.NextSibling; node != null; node = node.NextSibling)
        {
            // Stop when we hit <div class="clear"></div> since there is nothing after it
            if (node is IElement el &&
                el.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase) &&
                el.ClassList.Contains("clear"))
            {
                break;
            }

            // Preserve HTML exactly
            if (node is IElement element)
            {
                sb.Append(element.OuterHtml);
            }
            else
            {
                sb.Append(node.TextContent);
            }
        }

        return markdownConverter.Convert(sb.ToString());
    }

    public static string ExtractAllAfterTag(IDocument document, string tag, string text)
    {
        // Find the start node
        IElement? start = document.QuerySelectorAll(tag)
            .FirstOrDefault(el => el.TextContent == text);

        if (start == null)
            return string.Empty;

        StringBuilder sb = new();

        // Walk through following siblings
        for (INode? node = start.NextSibling; node != null; node = node.NextSibling)
        {
            // Stop when we hit <div class="clear"></div>
            if (node is IElement el &&
                el.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase) &&
                el.ClassList.Contains("clear"))
            {
                break;
            }

            // Preserve HTML exactly
            if (node is IElement element)
            {
                sb.Append(element.OuterHtml);
            }
            else
            {
                sb.Append(node.TextContent);
            }
        }

        return markdownConverter.Convert(sb.ToString());
    }

    public static string ExtractAllAfterTag(IDocument document, INode tag)
    {
        StringBuilder sb = new();

        // Walk through following siblings
        for (INode? node = tag.NextSibling; node != null; node = node.NextSibling)
        {
            // Stop when we hit <div class="clear"></div>
            if (node is IElement el &&
                el.TagName.Equals("DIV", StringComparison.OrdinalIgnoreCase) &&
                el.ClassList.Contains("clear"))
            {
                break;
            }

            // Preserve HTML exactly
            if (node is IElement element)
            {
                sb.Append(element.OuterHtml);
            }
            else
            {
                sb.Append(node.TextContent);
            }
        }

        return markdownConverter.Convert(sb.ToString());
    }

    internal static string ConvertToMarkdown(string html)
        => markdownConverter.Convert(html);

    static readonly Converter markdownConverter = new(new Config()
    {
        RemoveComments = true,
        UnknownTags = Config.UnknownTagsOption.Bypass
    });
}
