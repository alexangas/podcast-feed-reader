using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PodcastFeedReader.Readers;
using Xunit;

namespace PodcastFeedReader.Tests.Readers
{
    public class FeedReaderPerformanceTests
    {
        private const string TestDataPath = @"..\..\..\TestData";

        private readonly ILogger<FeedReader> _logger;

        public FeedReaderPerformanceTests()
        {
            _logger = A.Fake<ILogger<FeedReader>>();
        }

        [Trait("Category", "Performance")]
        [Fact]
        public async Task PerformanceTest_Valid_Feeds()
        {
            foreach (var feedFile in Directory.EnumerateFiles($@"{TestDataPath}\Valid", "*.xml"))
            {
                using (var feedStream = new FileStream(feedFile, FileMode.Open))
                using (var reader = new StreamReader(feedStream))
                {
                    var feedContents = reader.ReadToEnd();
                    reader.BaseStream.Position = 0;
                    var episodeCount = Regex.Matches(feedContents, "<item>").Count;

                    var feedReader = new FeedReader(reader, _logger);
                    await feedReader.SkipPreheader();
                    feedReader.ReadDocumentHeader();

                    var showXml = await feedReader.GetShowXmlAsync();
                    showXml.Should().NotBeNull();

                    XDocument episodeXml;
                    for (var episodeIndex = 0; episodeIndex < episodeCount; episodeIndex++)
                    {
                        episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                        episodeXml.Should().NotBeNull("because we have {0} and there should be {1}", episodeIndex + 1, episodeCount);
                    }
                    episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                    episodeXml.Should().BeNull();
                }
            }
        }
    }
}
