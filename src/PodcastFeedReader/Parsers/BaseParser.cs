using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using PodcastFeedReader.Helpers;

namespace PodcastFeedReader.Parsers
{
    public abstract class BaseParser
    {
        private static readonly string NamespaceStrings;

        static BaseParser()
        {
            var namespaceBuilder = new StringBuilder();
            foreach (var name in Namespaces.AllNamespaces)
                namespaceBuilder.Append($@" {name.Key}=""{name.Value}""");
            NamespaceStrings = namespaceBuilder.ToString();
        }

        protected static string PreParseCleanUp(string initialText)
        {
            var textBuilder = new StringBuilder(initialText);
            int startIndex;
            int endIndex;

            // Remove comments
            startIndex = initialText.IndexOf("<!--", StringComparison.Ordinal);
            while (startIndex > 0)
            {
                endIndex = textBuilder.IndexOf("-->", startIndex);
                if (endIndex > startIndex)
                    textBuilder.Remove(startIndex, endIndex - startIndex + "-->".Length);

                startIndex = textBuilder.IndexOf("<!--", Math.Min(startIndex, textBuilder.Length - 1));
            }

            // Replace invalid characters
            textBuilder.Replace("&nbsp;", "&#160;");
            textBuilder.Replace("<powerpress>", "");
            textBuilder.Replace("</powerpress>", "");
            textBuilder.Replace("<itunes:summary>", "");
            textBuilder.Replace("</itunes:summary>", "");

            // HTML doesn't have to have closing tags
            //// Ensure closing tag really is a closing tag
            //startIndex = text.LastIndexOf('<');
            //if (startIndex > 0 && text[startIndex + 1] != '/')
            //    text = text.Insert(startIndex + 1, "/");

            // Get first start tag
            startIndex = textBuilder.IndexOf("<");
            if (startIndex < 0)
                startIndex = 0;

            var endIndexTag = textBuilder.IndexOf(">", startIndex);
            var endIndexSpace = textBuilder.IndexOf(" ", startIndex);
            if (endIndexTag > 0 && endIndexTag < endIndexSpace)
            {
            }
            else if (endIndexSpace > 0 && endIndexSpace < endIndexTag && textBuilder[endIndexTag - 1] == '/')
            {
            }
            else if (endIndexSpace > 0 && endIndexSpace < endIndexTag)
            {
            }
            else
                return textBuilder.ToString();

            //if (text[startIndex + endIndex - 1] != '/')
            //{
            //    var snippet = text.Substring(startIndex + 1, endIndex - 1);
            //    var endSnippet = $"</{snippet}>";
            //    if (!text.Contains(endSnippet))
            //        text += endSnippet;
            //}

            var xmlString = $"<xml{NamespaceStrings}>{textBuilder}</xml>";
            XDocument xdoc;
            try
            {
                xdoc = XmlHelper.ReadXml(xmlString);
            }
            catch (XmlException xmlex)
            {
                if (xmlex.Message.Contains("does not match the end tag"))
                    return textBuilder.ToString();
                if (xmlex.Message.Contains("'=' is an unexpected token. The expected token is ';'."))
                    return textBuilder.ToString();
                var errorMessage = $"Error pre-parsing XML at '{xmlString.Substring(Math.Max(0, xmlex.LinePosition - 1), Math.Min(xmlex.LinePosition + 25, xmlString.Length - xmlex.LinePosition - 1))}'";
                throw new InvalidOperationException(errorMessage, xmlex);
            }
            return xdoc.Root?.Value;
        }
    }
}
