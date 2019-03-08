using System.Diagnostics;

namespace PodcastFeedReader.Model.Search
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ShowForSearch
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Description { get; set; }

        #if DEBUG
        private string DebuggerDisplay => $"Title: '{Title}'";
        #endif
    }
}