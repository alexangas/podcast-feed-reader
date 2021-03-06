﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PodcastFeedReader.Helpers
{
    public static class XmlHelper
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
    }
}
