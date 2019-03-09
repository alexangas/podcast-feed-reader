using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using PodcastFeedReader.Exceptions;
using PodcastFeedReader.Helpers;
using PodcastFeedReader.Model.DataAnnotations;
using PodcastFeedReader.Model.Display;
using PodcastFeedReader.Model.Parsed;
using PodcastFeedReader.Model.Search;
using PodcastFeedReader.Services;

namespace PodcastFeedReader.Sanitizers
{
    public class ShowSanitizer
    {
        private readonly ParsedShow _parsedShow;
        private readonly ISanitizationService _sanitizationService;
        private readonly IMetadataService _metadataService;

        public ShowSanitizer(
            ParsedShow parsedShow,
            ISanitizationService sanitizationService,
            IMetadataService metadataService
        )
        {
            if (parsedShow == null)
                throw new ArgumentNullException(nameof(parsedShow));
            if (sanitizationService == null)
                throw new ArgumentNullException(nameof(sanitizationService));
            if (metadataService == null)
                throw new ArgumentNullException(nameof(metadataService));

            _parsedShow = parsedShow;
            _sanitizationService = sanitizationService;
            _metadataService = metadataService;
        }

        public (ShowForDisplay, ICollection<string>) GetShowForDisplay(out ICollection<string> missingExpectedProperties)
        {
            var show = new ShowForDisplay
            {
                Title = SanitizeTitleForDisplay(),
                WebLink = SanitizeWebLink(),
                ImageLink = SanitizeImageLink(),
                Subtitle = SanitizeSubtitleForDisplay(),
                Description = SanitizeDescriptionForDisplay(),
                Author = SanitizeAuthor(),
                Language = SanitizeLanguage(),
                Tags = SanitizeTags().Select(x => new TagForDisplay(x)).ToList()
            };

            var missingRequiredProperties = _metadataService.GetPropertyNamesMissingValueByAttribute<ShowForDisplay, RequiredAttribute>(show);
            if (missingRequiredProperties.Any())
                throw new InvalidPropertiesException(missingRequiredProperties.ToDictionary(x => x, y => "Required"));

            missingExpectedProperties = _metadataService.GetPropertyNamesMissingValueByAttribute<ShowForDisplay, ExpectedAttribute>(show);

            return (show, missingExpectedProperties);
        }

        public ShowForSearch GetShowForSearch()
        {
            var showSearch = new ShowForSearch
            {
                Title = SanitizeTitleForSearch(),
                Subtitle = SanitizeSubtitleForSearch(),
                Description = SanitizeDescriptionForSearch()
            };

            return showSearch;
        }

        private string SanitizeTitleForDisplay()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Title))
                return null;

            var title = _sanitizationService.SanitizeToTextOnly(_parsedShow.Title).Trim();
            return title;
        }

        private string SanitizeTitleForSearch()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Title))
                return null;

            var titleForSearch = _sanitizationService.SanitizeToTextOnly(_parsedShow.Title).Trim();
            return titleForSearch;
        }

        private string SanitizeWebLink()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.WebLink))
                return null;

            var webUri = UriHelper.GetAbsoluteUri(_parsedShow.WebLink);
            return webUri;
        }

        private string SanitizeImageLink()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.ImageLink))
                return null;

            var imageUri = UriHelper.GetAbsoluteUri(_parsedShow.ImageLink);
            return imageUri;
        }

        private string SanitizeSubtitleForDisplay()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Subtitle))
                return null;

            var subtitle = _sanitizationService.SanitizeToTextOnly(_parsedShow.Subtitle).Trim();
            return subtitle;
        }

        private string SanitizeSubtitleForSearch()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Subtitle))
                return null;

            var subtitleForSearch = _sanitizationService.SanitizeToTextOnly(_parsedShow.Subtitle).Trim();
            return subtitleForSearch;
        }

        private string SanitizeDescriptionForDisplay()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Description))
                return null;

            var description = _sanitizationService.SanitizeToWebDisplay(_parsedShow.Description).Trim();
            return description;
        }

        private string SanitizeDescriptionForSearch()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Description))
                return null;

            var descriptionForSearch = _sanitizationService.SanitizeToTextOnly(_parsedShow.Description).Trim();
            return descriptionForSearch;
        }

        private string SanitizeAuthor()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Author))
                return null;

            var author = _sanitizationService.SanitizeToTextOnly(_parsedShow.Author).Trim();
            return author;
        }

        private string SanitizeLanguage()
        {
            if (String.IsNullOrWhiteSpace(_parsedShow.Language))
                return null;

            var language = _sanitizationService.SanitizeToTextOnly(_parsedShow.Language).Trim().ToLowerInvariant();
            return language;
        }

        private ICollection<string> SanitizeTags()
        {
            if (_parsedShow.Tags == null)
                return new List<string>();

            var tags = _parsedShow.Tags
                .Select(x => _sanitizationService.SanitizeToTextOnly(x.Name).Trim())
                .Select(x => x.IndexOf('&') >= 0 ? WebUtility.HtmlDecode(x) : x)
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Select(TagSanitizer.MakeValid)
                .Distinct()
                .ToList();
            return tags;
        }
    }
}
