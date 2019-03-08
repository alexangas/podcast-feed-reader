using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using PodcastFeedReader.Readers;
using Xunit;

namespace PodcastFeedReader.Tests.Readers
{
    public class PodcastFeedReaderTests
    {
        private const string TestDataRoot = @"TestData\";

        private readonly ILogger<PodcastFeedReader> _logger;

        public PodcastFeedReaderTests()
        {
            _logger = A.Fake<ILogger<PodcastFeedReader>>();
        }

        [Fact]
        public async Task SkipPreheader_Empty_Throws()
        {
            var input = "";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageOnly_Throws()
        {
            var input = "".PadRight(12, 'z');

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_Garbage_DoesNotThrow()
        {
            var input = "".PadRight(12, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageWithExpectedLength_DoesNotThrow()
        {
            var input = "".PadRight(12, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageLong_Throws()
        {
            var input = "".PadRight(10000, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_Html_Throws()
        {
            var input = "<html>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsXml_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsRss_DoesNotThrow()
        {
            var input = "<rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsXmlWithExpectedLength_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                Func<Task> act = async () => await podcastReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_ValidChannel_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                Action act = () => podcastReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_GarbageStartValidChannel_DoesNotThrow()
        {
            var input = "".PadRight(3, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                Action act = () => podcastReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_ValidChannelNonXml_DoesNotThrow()
        {
            var input = "<?xml ￾version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                Action act = () => podcastReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_InvalidChannel_Throws()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<flannel>";

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                Action act = () => podcastReader.ReadDocumentHeader();
                act.Should().Throw<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_PreChannelGarbage_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_prechannelgarbage.xml");

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_OneBufferContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_onebuffercontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_TwoBufferContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_twobuffercontent.xml");

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_TooLongContent_Throws()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_toolongcontent.xml");

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_AmpersandOutOfCDataContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_ampersandoutofcdatacontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }
        
        [Fact]
        public async Task GetShowAsync_AmpersandInsideOfCDataContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_ampersandinsideofcdatacontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_AmpersandAfterCDataContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_ampersandaftercdatacontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                Func<Task> act = async () => await podcastReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetNextEpisodeAsync_OneEpisode_Returns()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_oneepisodecontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                await podcastReader.GetShowXmlAsync();
                var xml = await podcastReader.GetNextEpisodeXmlAsync();
                xml.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetNextEpisodeAsync_TwoEpisodesContent_Returns()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_twoeppisodescontent.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                await podcastReader.GetShowXmlAsync();
                var xml = await podcastReader.GetNextEpisodeXmlAsync();
                xml.Should().NotBeNull();
                xml = await podcastReader.GetNextEpisodeXmlAsync();
                xml.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetNextEpisodeAsync_NoMoreEpisodes_ReturnsNull()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_nomoreepisodes.xml");

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                var podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                await podcastReader.GetShowXmlAsync();
                await podcastReader.GetNextEpisodeXmlAsync();
                var xml = await podcastReader.GetNextEpisodeXmlAsync();
                xml.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetNextEpisodeAsync_TooLongContent_Throws()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_toolongcontent2.xml");

            PodcastFeedReader podcastReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                podcastReader = new PodcastFeedReader(reader, _logger);
                await podcastReader.SkipPreheader();
                podcastReader.ReadDocumentHeader();
                await podcastReader.GetShowXmlAsync();
                Func<Task> act = async () => await podcastReader.GetNextEpisodeXmlAsync();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }
    }
}
