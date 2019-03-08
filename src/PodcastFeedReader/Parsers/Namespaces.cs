using System.Collections.Generic;
using System.Xml.Linq;

namespace PodcastFeedReader.Parsers
{
    public static class Namespaces
    {
        // ReSharper disable once InconsistentNaming
        public static readonly XNamespace ITunesNamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
        public static readonly XNamespace MediaNamespace = "http://search.yahoo.com/mrss/";
        public static readonly XNamespace DublinCoreNamespace = "http://purl.org/dc/elements/1.1/";

        public static readonly IDictionary<string, XNamespace> AllNamespaces =
            new Dictionary<string, XNamespace>
            {
                {"xmlns:itunes", ITunesNamespace},
                {"xmlns:media", MediaNamespace},
                {"xmlns:dc", DublinCoreNamespace}
            };
    }
}
