using System.Collections.Generic;

namespace PodFeedReader.Model.Parsed
{
    public class ParsedShow
    {
        public string Title { get; set; }
        public string WebLink { get; set; }
        public string ImageLink { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Language { get; set; }
        public ICollection<string> Tags { get; set; }
    }
}
