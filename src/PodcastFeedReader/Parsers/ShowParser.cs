using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using PodcastFeedReader.Model.Parsed;
using PodcastFeedReader.Readers;

namespace PodcastFeedReader.Parsers
{
    public class ShowParser : BaseParser, IContentParser<ParsedShow>
    {
        private ParsedShow _content = null!;

        public ParsedShow Content
        {
            get => _content == null ? throw new InvalidOperationException("Content has not been parsed") : _content;
            set => _content = value;
        }

        public void ParseFromXml(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var title = GetTitle(element);
            var parsedTags = GetTags(element).Select(x => new ParsedTag(x)).ToList();
            Content = new ParsedShow(title, parsedTags)
            {
                WebLink = GetWebLink(element),
                ImageLink = GetImageLink(element),
                Subtitle = GetSubtitle(element),
                Description = GetDescription(element),
                Author = GetAuthor(element),
                Language = GetLanguage(element),
            };
        }

        private static string GetTitle(XElement doc)
        {
            var title = doc.Descendants("title").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(title))
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.NoShowTitle);
            return title;
        }

        private static string? GetWebLink(XElement doc)
        {
            var webUrl = doc.Descendants("link").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(webUrl))
                return null;
            webUrl = webUrl.Trim();
            return webUrl;
        }

        private static string? GetImageLink(XElement doc)
        {
            var imageUrl = DescendantsCaseInsensitive(doc, Namespaces.ITunesNamespace + "image").Attributes("href").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = doc.Descendants("image").Elements("url").Select(x => x.Value).FirstOrDefault();
                if (String.IsNullOrWhiteSpace(imageUrl))
                    return null;
            }
            imageUrl = imageUrl.Trim();
            return imageUrl;
        }

        private static string? GetSubtitle(XElement doc)
        {
            var summary = doc.Descendants("description").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(summary))
                return summary;

            summary = DescendantsCaseInsensitive(doc, Namespaces.ITunesNamespace + "subtitle").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(summary))
                return null;

            return summary;
        }

        private static string? GetDescription(XElement doc)
        {
            var description = DescendantsCaseInsensitive(doc, Namespaces.ITunesNamespace + "summary").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(description))
            {
                if (description.IndexOf("<", StringComparison.OrdinalIgnoreCase) == 0)
                    description = PreParseCleanUp(description);
                return description;
            }

            description = doc.Descendants("description").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(description))
                return null;

            return description;
        }

        private static string? GetAuthor(XElement doc)
        {
            var author = DescendantsCaseInsensitive(doc, Namespaces.ITunesNamespace + "author").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(author))
                return null;
            return author;
        }

        private static string? GetLanguage(XElement doc)
        {
            var language = doc.Descendants("language").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(language))
                return language;

            language = doc.Descendants(Namespaces.DublinCoreNamespace + "language").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(language))
                return null;

            return language;
        }

        private static ICollection<string> GetTags(XElement doc)
        {
            var channelElement = doc.Descendants("channel").FirstOrDefault();
            if (channelElement == null)
                return new List<string>(0);

            var keywords = ElementsCaseInsensitive(channelElement, Namespaces.ITunesNamespace + "keywords").Select(x => x.Value).FirstOrDefault()?.Split(',')
                           ?? new string[0];

            var categories = ElementsCaseInsensitive(channelElement, Namespaces.ITunesNamespace + "category").DescendantsAndSelf().Attributes("text").Select(x => x.Value);

            var categories2 = ElementsCaseInsensitive(channelElement, "category").Select(x => x.Value);

            var tagsRaw = keywords
                .Union(categories)
                .Union(categories2)
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();

            return tagsRaw;
        }
    }
}
