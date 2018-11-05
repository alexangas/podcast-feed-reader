using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace PodApp.Data.Collection.Helpers.Xml
{
    public static class XmlHelper
    {
        public static XDocument Parse(string raw)
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
                try
                {
                    xDocument = XDocument.Load(xmlReader, LoadOptions.SetLineInfo);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return xDocument;
        }

        // From https://stackoverflow.com/a/9335141/6651
        public static IEnumerable<XElement> ElementsCaseInsensitive(this XContainer source, XName name)
        {
            foreach (XElement e in source.Elements())
            {
                if (e.Name.Namespace.NamespaceName.Equals(name.Namespace.NamespaceName, StringComparison.OrdinalIgnoreCase) &&
                    e.Name.LocalName.Equals(name.LocalName, StringComparison.OrdinalIgnoreCase))
                    yield return e;
            }
        }

        public static IEnumerable<XElement> DescendantsCaseInsensitive(this XContainer source, XName name)
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