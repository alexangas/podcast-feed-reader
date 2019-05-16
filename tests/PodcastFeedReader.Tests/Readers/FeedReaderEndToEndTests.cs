using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PodcastFeedReader.Readers;
using Xunit;
using Xunit.Abstractions;

namespace PodcastFeedReader.Tests.Readers
{
    public class FeedReaderEndToEndTests
    {
        private const string TestDataPath = @"..\..\..\TestData";

        private readonly ITestOutputHelper _testOutputHelper;

        private readonly ILogger<FeedReader> _logger;

        public FeedReaderEndToEndTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = A.Fake<ILogger<FeedReader>>();
        }

        [Trait("Category", "Performance")]
        [Theory]
        [MemberData(nameof(ValidFeeds))]
        public async Task EndToEnd_Valid_Feeds(string feedFile)
        {
            using (var feedStream = new FileStream(feedFile, FileMode.Open))
            using (var reader = new StreamReader(feedStream))
            {
                var feedContents = reader.ReadToEnd();
                reader.BaseStream.Position = 0;
                var episodeCount = Regex.Matches(feedContents, @"\</item\>").Count;

                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();

                var showXml = await feedReader.GetShowXmlAsync();
                showXml.Should().NotBeNull();
                var showTitle = Regex.Match(showXml.ToString(), @"\<title\>([^<]+)\<").Groups[1].Value;
                _testOutputHelper.WriteLine($"Show: '{showTitle}'");

                XDocument episodeXml = null;
                for (var episodeIndex = 0; episodeIndex < episodeCount; episodeIndex++)
                {
                    var lastXml = episodeXml;
                    episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                    if (episodeXml != null)
                    {
                        var episodeTitle = Regex.Match(episodeXml.ToString(), @"\<title\>([^<]+)\<").Groups[1].Value;
                        _testOutputHelper.WriteLine($"Episode index {episodeIndex + 1}: '{episodeTitle}'");
                    }

                    episodeXml.Should().NotBeNull($"because we have {episodeIndex + 1} and there should be {episodeCount}, previous with content: {lastXml}");
                }

                episodeXml = await feedReader.GetNextEpisodeXmlAsync();
                if (episodeXml != null)
                    _testOutputHelper.WriteLine($"Episode unexpected: '{Regex.Match(episodeXml.ToString(), @"\<title\>([^<]+)\<").Groups[1].Value}'");
                episodeXml.Should().BeNull($"because we do not expect more in {feedFile} after {episodeCount} episodes");
            }
        }

        public static IEnumerable<object[]> ValidFeeds() =>
            Directory.EnumerateFiles($@"{TestDataPath}\Valid", "*.xml").Select(feedFile => new object[] {feedFile});
    }
}
