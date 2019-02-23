using System;

namespace PodFeedReader.Readers
{
    [Serializable]
    public class InvalidPodcastFeedException : Exception
    {
        public string Contents { get; set; }

        public enum InvalidPodcastFeedReason : short
        {
            Unknown = 0,
            UnexpectedEndOfStream = 1,
            UnexpectedByteCount = 2,
            HtmlDocument = 3,
            FeedStartNotFound = 4,
            ShowStartNotFound = 5,
            EpisodeStartNotFound = 5,
            ShowContentTooLong = 7,
            EpisodeContentTooLong = 7,
        }

        public InvalidPodcastFeedException(InvalidPodcastFeedReason reason) : base($"{reason}")
        {
        }

        public InvalidPodcastFeedException(InvalidPodcastFeedReason reason, string contents) : this(reason)
        {
            Contents = contents;
        }
    }
}
