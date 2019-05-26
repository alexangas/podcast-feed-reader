using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using PodcastFeedReader.Parsers;
using Xunit;

namespace PodcastFeedReader.Tests.Parsers
{
    public class EpisodeParserTests
    {
        private const string TestDataRoot = @"TestData\";

        [Fact]
        public void GetContent_WithoutCallingParse_ReturnsNull()
        {
            var parser = new EpisodeParser();

            var result = parser.GetContent();

            result.Should().BeNull();
        }

        [Fact]
        public void ParseFromXml_WithNoContents_HasEmptyContent()
        {
            var doc = XDocument.Parse("<xml></xml>").Root;
            var parser = new EpisodeParser();

            parser.ParseFromXml(doc);

            var episode = parser.GetContent();
            episode.Should().BeNull();
        }

        [Fact]
        public void ParseFromXml_WithContents_ReturnsExpected()
        {
            var text = File.ReadAllText($@"{TestDataRoot}Valid\samplefeed1.xml");
            var doc = XDocument.Parse(text);
            var parser = new EpisodeParser();

            parser.ParseFromXml(doc.Descendants("item").First());

            var episode = parser.GetContent();
            episode.AudioLink.Should().Be("https://justsayinpodcast.files.wordpress.com/2015/11/ep-16-rubbing-the-desk.mp3");
            episode.Title.Should().Be("EP 16: Rubbing The Desk");
            episode.WebLink.Should().Be("https://justsayinpodcast.wordpress.com/2015/11/21/ep-16-rubbing-the-desk/");
            episode.ImageLink.Should().Be("https://0.gravatar.com/avatar/6d3c746bf2738f291839c6561f9c08b1?s=96&d=identicon&r=G");
            episode.CommentsLink.Should().Be("https://justsayinpodcast.wordpress.com/2015/11/21/ep-16-rubbing-the-desk/#respond");
            episode.PublishDate.Should().Be(new DateTime(2015, 11, 21, 01, 58, 00, DateTimeKind.Utc));
            episode.Description.Should().Be("*Warning Adult Language* In this episode we discuss why you shouldn&#8217;t Black Friday shop, Supermega fest Convention, Sequels that shouldn&#8217;t have happened, and bad Boston accents in film, and the magic of Bob Ross. &#160;");
            episode.Author.Should().Be("Ryan & Tara");
            episode.Duration.Should().BeNull();
            episode.AudioSize.Should().Be(57671328);
        }
    }
}
