using System.Xml.Linq;

namespace PodFeedReader.Parsers
{
    public interface IContentParser<out TParsedContent>
    {
        void ParseFromXml(XElement element);

        TParsedContent GetContent();
    }
}
