using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using PodcastFeedReader.Model.DataAnnotations;

namespace PodcastFeedReader.Model.Display
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ShowForDisplay
    {
        [Required]
        public string Title { get; set; }

        [Expected]
        public string WebLink { get; set; }
        
        public string ImageLink { get; set; }
        
        public string Subtitle { get; set; }
        
        [Expected]
        public string Description { get; set; }
        
        public string Author { get; set; }
        
        public string Language { get; set; }
        
        public ICollection<TagForDisplay> Tags { get; set; }

        #if DEBUG
        private string DebuggerDisplay => $"Title: '{Title}'";
        #endif
    }
}
