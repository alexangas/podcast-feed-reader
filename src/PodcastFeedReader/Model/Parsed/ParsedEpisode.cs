using System;
using System.Diagnostics;

namespace PodcastFeedReader.Model.Parsed
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class ParsedEpisode
    {
        public string Title { get; set; }

        public string WebLink { get; set; }

        public string ImageLink { get; set; }

        public string CommentsLink { get; set; }

        public string AudioLink { get; set; }

        public DateTime? PublishDate { get; set; }

        public string Description { get; set; }

        public string Author { get; set; }

        public DateTime? Duration { get; set; }

        public long? AudioSize { get; set; }        

#if DEBUG
        private string DebuggerDisplay => $"Title: '{Title}'";
#endif
    }
}