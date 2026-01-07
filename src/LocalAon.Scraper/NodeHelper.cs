using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ReverseMarkdown;
using ReverseMarkdown.Converters;

namespace LocalAon.Scraper;

internal static class NodeHelper
{
    // Node grabbers
    internal static IElement? GetSourceNode(IElement parentNode)
        => parentNode.QuerySelector("a.external-link");

    internal static string GetSourceText(IElement parentNode)
        => GetSourceNode(parentNode)?
            .TextContent ?? string.Empty;

    // Text grabbers

    /// <summary>
    /// Finds the B node with boldNodeText and returns the text after it. Use gatherRestOfLine if the text after it
    /// contains other tags.
    /// </summary>
    internal static string? GetTextAfterBoldNode(IDocument document, string boldNodeText, bool gatherRestOfLine = false)
    {
        IElement? start = document.All
            .FirstOrDefault(e =>
                (e.LocalName == "b") &&
                e.TextContent.Trim().Equals(boldNodeText, StringComparison.OrdinalIgnoreCase));

        if (start == null)
        {
            return string.Empty;
        }

        string output = Normalize(gatherRestOfLine
            ? GetLineContent(start)
            : GetNextTextNode(start));

        return string.IsNullOrWhiteSpace(output)
            ? null
            : output;
    }

    /// <summary>
    /// Get the text representation of the text after startNode but changes the followup nodes to text when they are html
    /// </summary>
    static string GetLineContent(INode startNode)
    {
        StringBuilder sb = new();
        INode? current = startNode.NextSibling;

        while (current != null)
        {
            if (current is IHtmlBreakRowElement || current.NodeName.Equals("BR", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (current is IElement { LocalName: "b" })
            {
                break;
            }

            sb.Append(current.TextContent);

            current = current.NextSibling;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the text afte specified node
    /// </summary>
    static string GetNextTextNode(INode node)
    {
        for (INode? n = node.NextSibling; n != null; n = n.NextSibling)
        {
            if (n is IText text && !string.IsNullOrWhiteSpace(text.Data))
                return Normalize(text.Data);
        }

        return string.Empty;
    }

    internal static string Normalize(string text)
    {
        text = text.Trim();

        text = text.Replace("\u00A0", " ");

        if (text.StartsWith(':'))
        {
            text = text[1..];
        }

        if (text.EndsWith(':'))
        {
            text = text[..^1];
        }

        if (GetCharacterCount(text, ';') == 1 && text.EndsWith(';'))
        {
            text = text[..^1];
        }

        return text.Trim();
    }

    static int GetCharacterCount(string text, char charToFind)
    {
        int count = 0;

        foreach (char c in text.AsSpan())
        {
            if (c == charToFind)
                count++;
        }

        return count;
    }

    /// <summary>
    /// Checks whether a given node counts as the end of the page (div.clear)
    /// </summary>
    internal static bool IsEndOfPageNode(INode node)
    {
        return node is IElement el &&
            el.TagName.Equals("div", StringComparison.OrdinalIgnoreCase) &&
            el.ClassList.Contains("clear");
    }

    /// <summary>
    /// Gets all text after specified node until it either reaches end of page (div.clear)
    /// TODO: Custom end node or end tag
    /// </summary>
    /// <param name="startNode">The node right before the text</param>
    /// <param name="preserveTags">If set to false, ignore tags. When set to true the output will include tags (excluding br)</param>
    /// <param name="asMarkdown">If set to true, convert html tags to markdown.</param>
    /// <returns></returns>
    internal static string GetAllTextAfterNode(INode? startNode,
        INode? stopAtNode = null, bool preserveTags = false, bool asMarkdown = false)
    {
        if (startNode == null)
        {
            return string.Empty;
        }

        StringBuilder sb = new();

        INode? currNode = startNode.NextSibling;
        sb.AppendLine();

        while (currNode != null && IsEndOfPageNode(currNode) == false)
        {
            if (currNode == stopAtNode)
                break;

            IElement? el = currNode as IElement;

            if (el != null && el.TagName.Equals("br", StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine();
            }

            if (el != null && el.TagName.Equals("i", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append($"{el.TextContent}: ");
            }

            if (preserveTags
                && currNode is IElement element
                && element.TagName.Equals("br", StringComparison.OrdinalIgnoreCase) == false)
            {
                sb.Append(element.OuterHtml);
            }
            else
            {
                sb.Append(currNode.TextContent);
            }

            currNode = currNode.NextSibling;
        }

        return asMarkdown == false
            ? Normalize(sb.ToString())
            : ConvertToMarkdown(Normalize(sb.ToString()));
    }

    internal static string ConvertToMarkdown(string html)
        => markdownConverter.Convert(html);

    static readonly Converter markdownConverter = new(new Config()
    {
        RemoveComments = true,
        UnknownTags = Config.UnknownTagsOption.Bypass
    });
}
