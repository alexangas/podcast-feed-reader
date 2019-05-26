using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using PodcastFeedReader.Helpers;
using PodcastFeedReader.Model.Parsed;

namespace PodcastFeedReader.Parsers
{
    public class EpisodeParser : BaseParser, IContentParser<ParsedEpisode>
    {
        private ParsedEpisode _content;

        public void ParseFromXml(XElement element)
        {
            _content = null;

            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var audioLink = GetAudioLink(element);
            if (String.IsNullOrWhiteSpace(audioLink))
                return;

            _content = new ParsedEpisode
            {
                AudioLink = audioLink,
                Title = GetTitle(element),
                WebLink = GetWebLink(element),
                ImageLink = GetImageLink(element),
                CommentsLink = GetCommentsLink(element),
                PublishDate = GetPublishDate(element),
                Description = GetDescription(element),
                Author = GetAuthor(element),
                Duration = GetDuration(element),
                AudioSize = GetAudioSize(element)
            };
        }

        public ParsedEpisode GetContent()
        {
            return _content;
        }

        private static string GetAudioLink(XElement element)
        {
            var audioUrl = element
                .Descendants("enclosure")
                .Attributes("url")
                .Select(x => x.Value)
                .FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(audioUrl))
                return audioUrl.Trim();

            var audioElement = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.MediaNamespace + "content")
                .Where(x => x != null)
                .Attributes("medium")
                .FirstOrDefault(x => x != null && x.Value == "audio");
            var parent = audioElement?.Parent;
            if (parent == null)
                return null;
            var urlAttribute = parent.Attributes("url").FirstOrDefault();
            audioUrl = urlAttribute?.Value;
            if (String.IsNullOrWhiteSpace(audioUrl))
                return audioUrl;
            return audioUrl.Trim();
        }

        private static string GetTitle(XElement element)
        {
            var title = element.Descendants("title").Select(x => x.Value).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(title))
                title = GetSubtitle(element);

            return title;
        }

        private static string GetWebLink(XElement element)
        {
            var linkUrl = element.Descendants("link").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(linkUrl))
                return linkUrl;
            return linkUrl.Trim();
        }

        private static string GetImageLink(XElement element)
        {
            var imageItunes = element
                .Descendants(Namespaces.ITunesNamespace + "image")
                .Attributes("href")
                .Where(x => x != null)
                .Select(x => x.Value)
                .FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(imageItunes))
                return imageItunes.Trim();

            var imageElement = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.MediaNamespace + "content")
                .Where(x => x != null)
                .Attributes("medium")
                .FirstOrDefault(x => x != null && x.Value == "image");
            var parent = imageElement?.Parent;
            if (parent == null)
                return null;
            var urlAttribute = parent.Attributes("url").FirstOrDefault();
            var imageUrl = urlAttribute?.Value;
            if (String.IsNullOrWhiteSpace(imageUrl))
                return imageUrl;
            return imageUrl.Trim();
        }

        private static string GetCommentsLink(XElement element)
        {
            var commentsUrl = element.Descendants("comments").Select(x => x.Value).FirstOrDefault();
            if (String.IsNullOrWhiteSpace(commentsUrl))
                return commentsUrl;
            commentsUrl = commentsUrl.Trim();
            return commentsUrl;
        }

        
        private static DateTime? GetPublishDate(XElement element)
        {
            var pubDateText = element.Descendants("pubDate").Select(x => x.Value).FirstOrDefault();
            if (pubDateText == null)
                return null;
            var pubDateTextTrimmed = pubDateText.Trim();
            var cultureInfo = CultureInfo.InvariantCulture;
            var dateTimeStyles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;
            if (!DateTime.TryParse(pubDateTextTrimmed, cultureInfo, dateTimeStyles, out var pubDate))
            {
                // Remove unwanted characters
                pubDateTextTrimmed = Regex.Replace(pubDateTextTrimmed,
                    @"(\w)(\.)",
                    "$1",
                    RegexOptions.Compiled)
                    .TrimEnd();

                if (!DateTime.TryParse(pubDateTextTrimmed, cultureInfo, dateTimeStyles, out pubDate))
                {
                    // Remove days of the week
                    pubDateTextTrimmed = Regex.Replace(pubDateTextTrimmed,
                        @"(Monday|Tuesday|Wednesday|Thursday|Friday|Saturday|Sunday|Thurs|Tues|Thur|Mon|Tue|Wed|Thu|Fri|Sat|Sun)[\.,]*",
                        String.Empty,
                        RegexOptions.Compiled)
                        .TrimStart();

                    if (!DateTime.TryParse(pubDateTextTrimmed, cultureInfo, dateTimeStyles, out pubDate))
                    {
                        // Correct months of the year
                        pubDateTextTrimmed = Regex.Replace(pubDateTextTrimmed,
                            @"\d\s*([A-Za-z]{4,})\s*\d",
                                match =>
                                {
                                    var month = match.Value;
                                    month = month.Replace("Sept", "Sep");
                                    return month;
                                },
                            RegexOptions.Compiled);

                        if (!DateTime.TryParse(pubDateTextTrimmed, cultureInfo, dateTimeStyles, out pubDate))
                        {
                            // Change timezone code to offset
                            pubDateTextTrimmed = Regex.Replace(pubDateTextTrimmed,
                                @"[A-Z]{3}$",
                                match =>
                                {
                                    var timeZone = match.Value;
                                    if (!DateTimeLists.TimeZones.TryGetValue(timeZone, out var offset))
                                    {
                                        // This will never be precisely accurate due to changes in time zones but can't do much else
                                        return String.Empty;
                                    }
                                    return offset;
                                },
                                RegexOptions.Compiled);

                            // Try and explicitly parse against US date format
                            if (!DateTime.TryParse(pubDateTextTrimmed, cultureInfo, dateTimeStyles, out pubDate))
                            {
                                var culture = CultureInfo.CreateSpecificCulture("en-US");
                                if (!DateTime.TryParse(pubDateTextTrimmed, culture, dateTimeStyles, out pubDate))
                                    return null;
                            }
                        }
                    }
                }
            }
            return pubDate;
        }

        private static string GetDescription(XElement element)
        {
            string description;

            description = element.Descendants("description").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(description))
            {
                if (description.IndexOf("<", StringComparison.OrdinalIgnoreCase) == 0)
                    description = PreParseCleanUp(description);
                if (!String.IsNullOrWhiteSpace(description))
                    return description;
            }

            description = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "summary").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(description))
                return description;

            description = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "subtitle").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(description))
                return description;

            return null;
        }

        private static string GetSubtitle(XElement element)
        {
            string subtitle;

            subtitle = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "subtitle").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(subtitle))
                return subtitle;

            subtitle = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "summary").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(subtitle))
                return subtitle;

            subtitle = element.Descendants("description").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(subtitle))
            {
                if (subtitle.IndexOf("<", StringComparison.OrdinalIgnoreCase) == 0)
                    subtitle = PreParseCleanUp(subtitle);
                if (!String.IsNullOrWhiteSpace(subtitle))
                    return subtitle;
            }

            return null;
        }

        private static string GetAuthor(XElement element)
        {
            var author = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "author").Select(x => x.Value).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(author))
                return author;

            author = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.DublinCoreNamespace + "creator").Select(x => x.Value).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(author))
                return null;

            return author;
        }

        private static DateTime? GetDuration(XElement element)
        {
            var durationText = XmlHelper.DescendantsCaseInsensitive(element, Namespaces.ITunesNamespace + "duration").Select(x => x.Value).FirstOrDefault();
            if (durationText == null)
                return null;
            var durationTextTrim = durationText.Trim();

            if (durationTextTrim.Contains(':') || durationTextTrim.Contains('.'))
            {
                var duration = GetDurationAsTimeSpan(durationTextTrim);
                if (duration != null)
                    return new DateTime(2010, 1, 1).Add(duration.Value);
            }

            if (!long.TryParse(durationTextTrim, out var durationSeconds))
                return null;
            return new DateTime(2010, 1, 1).AddSeconds(durationSeconds);
        }

        private static TimeSpan? GetDurationAsTimeSpan(string durationTextTrim)
        {
            if (TimeSpan.TryParse(durationTextTrim, out var duration))
                return duration;
            if (TimeSpan.TryParseExact(durationTextTrim, @"mm\:ss", CultureInfo.InvariantCulture, out duration))
                return duration;
            if (TimeSpan.TryParseExact(durationTextTrim, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out duration))
                return duration;
            if (TimeSpan.TryParseExact(durationTextTrim, @"mm\.ss", CultureInfo.InvariantCulture, out duration))
                return duration;
            if (!TimeSpan.TryParseExact(durationTextTrim, @"hh\.mm\.ss", CultureInfo.InvariantCulture, out duration))
                return null;
            return duration;
        }

        private static long? GetAudioSize(XElement element)
        {
            var audioSizeText = element.Descendants("enclosure").Attributes("length").Select(x => x.Value).FirstOrDefault();
            if (audioSizeText == null)
                return null;
            if (!long.TryParse(audioSizeText.Trim(), out var audioSize))
                return null;
            return audioSize;
        }
    }
}