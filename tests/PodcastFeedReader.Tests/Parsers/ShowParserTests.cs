using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FluentAssertions;
using PodcastFeedReader.Model.Parsed;
using PodcastFeedReader.Parsers;
using Xunit;

namespace PodcastFeedReader.Tests.Parsers
{
    public class ShowParserTests
    {
        private const string TestDataRoot = @"TestData\";

        [Fact]
        public void GetContent_WithoutCallingParse_Throws()
        {
            var parser = new ShowParser();

            var act = new Func<ParsedShow>(() => parser.GetContent());

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ParseFromXml_WithNoContents_HasEmptyContent()
        {
            var doc = XDocument.Parse("<xml></xml>").Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Title.Should().BeNull();
            show.WebLink.Should().BeNull();
            show.ImageLink.Should().BeNull();
            show.Subtitle.Should().BeNull();
            show.Description.Should().BeNull();
            show.Author.Should().BeNull();
            show.Language.Should().BeNull();
            show.Tags.Should().BeNull();
        }

        [Fact]
        public void ParseFromXml_WithContents_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Valid\samplefeed1.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Title.Should().Be("#JustSayin");
            show.WebLink.Should().Be("https://justsayinpodcast.wordpress.com");
            show.ImageLink.Should().Be("http://justsayinpodcast.files.wordpress.com/2015/07/cropped-podcast-logo.jpg");
            show.Subtitle.Should().Be("A Comedy Podcast");
            show.Description.Should().Be("#justSayin is a random comedy podcast, with Boston accents and a variety of subjects.");
            show.Author.Should().Be("Ryan & Tara");
            show.Language.Should().Be("en");
            show.Tags.Should().BeEquivalentTo(new List<string>(new[] {"Comedy", "Entertainment", "Boston", "Massachusetts", "Random", "Arts", "Performing Arts", "Games & Hobbies", "Video Games" }));
        }

        [Fact]
        public void ParseFromXml_Author_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_authormissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Author.Should().Be("CBC Radio");
        }

        [Fact]
        public void ParseFromXml_SubtitleOnlyDescription_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_authormissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Subtitle.Should().StartWith("CBC Radio's Leigh");
        }

        [Fact]
        public void ParseFromXml_SubtitleOnlyItunes_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_subtitlemissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Subtitle.Should().NotBeEmpty();
        }

        [Fact]
        public void ParseFromXml_SubtitleDescriptionAndItunes_PrefersDescription()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_descriptionmissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Subtitle.Should().StartWith("Storytelling im Radio");
        }

        [Fact]
        public void ParseFromXml_DescriptionOnlySummary_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_subtitlemissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Description.Should().StartWith("\nWatch award-winning documentary videos");
        }

        [Fact]
        public void ParseFromXml_DescriptionOnlyDescription_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_descriptionmissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Description.Should().StartWith("Storytelling im Radio");
        }

        [Fact]
        public void ParseFromXml_DescriptionSummaryAndDescription_PrefersSummary()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Valid\samplefeed1.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Description.Should().StartWith("#justSayin");
        }

        [Fact]
        public void ParseFromXml_ImageOnlyHref_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_conversations.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.ImageLink.Should().Be("http://www.abc.net.au/cm/rimage/7950252-1x1-large.jpg?v=4");
        }

        [Fact]
        public void ParseFromXml_ImageOnlyUrl_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_imagemissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.ImageLink.Should().Be("http://www.blogtalkradio.com/img/btrbetalogo.gif");
        }

        [Fact]
        public void ParseFromXml_ImageHrefAndUrl_PrefersHref()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_conversations.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.ImageLink.Should().Be("http://www.abc.net.au/cm/rimage/7950252-1x1-large.jpg?v=4");
        }

        [Fact]
        public void ParseFromXml_Tags_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_tagsmissing.xml");
            var doc = XDocument.Parse(text).Root;
            var parser = new ShowParser();

            parser.ParseFromXml(doc);

            var show = parser.GetContent();
            show.Tags.Should().BeEquivalentTo(new List<string>(new[] { "iTunes U", "Business", "Finance" }));
        }
    }
}
