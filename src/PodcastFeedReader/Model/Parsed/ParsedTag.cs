namespace PodcastFeedReader.Model.Parsed
{
    public class ParsedTag
    {
        /// <summary>
        /// Unsanitized tag name.
        /// </summary>
        public string Name { get; set; }

        public ParsedTag(string name)
        {
            Name = name;
        }
    }
}