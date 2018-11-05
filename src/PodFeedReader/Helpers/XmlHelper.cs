using System;
using System.IO;
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
    }
}