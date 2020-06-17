using System.Collections.Generic;
using System.Diagnostics;

namespace PodcastFeedReader.Model.Parsed
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class ParsedShow
    {
        public string Title { get; set; }

        public string? WebLink { get; set; }

        public string? ImageLink { get; set; }

        public string? Subtitle { get; set; }

        public string? Description { get; set; }

        public string? Author { get; set; }

        public string? Language { get; set; }

        public ICollection<ParsedTag> Tags { get; set; }

        public ParsedShow(string title, ICollection<ParsedTag> tags)
        {
            Title = title;
            Tags = tags;
        }

        #if DEBUG
        private string DebuggerDisplay => $"Title: '{Title}'";
        #endif
    }
}
