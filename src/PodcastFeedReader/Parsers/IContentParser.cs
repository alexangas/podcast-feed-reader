using System.Xml.Linq;

namespace PodcastFeedReader.Parsers
{
    public interface IContentParser<out TParsedContent>
    {
        void ParseFromXml(XElement element);

        TParsedContent Content { get; }
    }
}
