using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PodcastFeedReader.Helpers
{
    internal static class XmlHelper
    {
        public static XDocument ReadXml(string raw)
        {
            XDocument xDocument;

            var xmlReaderSettings = new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                ValidationType = ValidationType.None,
            };

            using (var stringReader = new StringReader(raw))
            using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
            {
                xmlReader.MoveToContent();
                xDocument = XDocument.Load(xmlReader, LoadOptions.SetLineInfo);
            }

            return xDocument;
        }

        public static IEnumerable<XElement> ElementsCaseInsensitive(XContainer source, XName name)
        {
            // From https://stackoverflow.com/a/9335141/6651

            foreach (XElement e in source.Elements())
            {
                if (e.Name.Namespace.NamespaceName.Equals(name.Namespace.NamespaceName, StringComparison.OrdinalIgnoreCase) &&
                    e.Name.LocalName.Equals(name.LocalName, StringComparison.OrdinalIgnoreCase))
                    yield return e;
            }
        }

        public static IEnumerable<XElement> DescendantsCaseInsensitive(XContainer source, XName name)
        {
            foreach (XElement e in source.Descendants())
            {
                if (e.Name.Namespace.NamespaceName.Equals(name.Namespace.NamespaceName, StringComparison.OrdinalIgnoreCase) &&
                    e.Name.LocalName.Equals(name.LocalName, StringComparison.OrdinalIgnoreCase))
                    yield return e;
            }
        }
    }
}
