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

                    XDocument episodeXml = null;
                    for (var episodeIndex = 0; episodeIndex < episodeCount - 1; episodeIndex++)
                    {
                        var lastXml = episodeXml;
                        episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                        episodeXml.Should().NotBeNull($"because we have {episodeIndex + 1} and there should be {episodeCount}, last was in {feedFile} with content {lastXml?.ToString()}");
                    }
                    episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                    episodeXml.Should().BeNull($"because we do not expect more in {feedFile} after {episodeCount} episodes");
                }
            }
        }
    }
}
