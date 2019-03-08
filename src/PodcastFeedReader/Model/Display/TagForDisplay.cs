using System;

namespace PodcastFeedReader.Model.Display
{
    public class TagForDisplay
    {
        /// <summary>
        /// Sanitized tag name.
        /// </summary>
        public string Name { get; set; }

        public TagForDisplay(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            Name = name;
        }
    }
}
