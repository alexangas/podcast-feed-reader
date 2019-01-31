using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PodFeedReader.Readers;
using Xunit;

namespace PodFeedReader.Tests.Readers
{
    public class PodcastFeedReaderPerformanceTests
    {
        private const string TestDataPath = @"..\..\..\TestData";

        private readonly ILogger<PodcastFeedReader> _logger;

        public PodcastFeedReaderPerformanceTests()
        {
            _logger = A.Fake<ILogger<PodcastFeedReader>>();
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

                    var podcastFeedReader = new PodcastFeedReader(reader, _logger);
                    await podcastFeedReader.SkipPreheader();
                    podcastFeedReader.ReadDocumentHeader();

                    var showXml = await podcastFeedReader.GetShowXmlAsync();
                    showXml.Should().NotBeNull();

                    XDocument episodeXml;
                    for (var episodeIndex = 0; episodeIndex < episodeCount; episodeIndex++)
                    {
                        episodeXml = await podcastFeedReader.GetNextEpisodeXmlAsync();
                        episodeXml.Should().NotBeNull("because we have {0} and there should be {1}", episodeIndex + 1, episodeCount);
                    }
                    episodeXml = await podcastFeedReader.GetNextEpisodeXmlAsync();
                    episodeXml.Should().BeNull();
                }
            }
        }
    }
}
