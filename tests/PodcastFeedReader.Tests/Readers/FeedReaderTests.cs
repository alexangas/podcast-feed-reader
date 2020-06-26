using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using PodcastFeedReader.Readers;
using Xunit;

namespace PodcastFeedReader.Tests.Readers
{
    public class FeedReaderTests
    {
        private const string TestDataRoot = @"TestData\";

        [Theory]
        [InlineData(@"Valid\samplefeed1\samplefeed1.xml", @"Valid\samplefeed1\samplefeed1.header.xml")]
        [InlineData(@"Valid\samplefeed2\samplefeed2.xml", @"Valid\samplefeed2\samplefeed2.header.xml")]
        [InlineData(@"Valid\samplefeed3\samplefeed3.xml", @"Valid\samplefeed3\samplefeed3.header.xml")]
        [InlineData(@"Valid\samplefeed4\samplefeed4.xml", @"Valid\samplefeed4\samplefeed4.header.xml")]
        [InlineData(@"Valid\samplefeed5\samplefeed5.xml", @"Valid\samplefeed5\samplefeed5.header.xml")]
        [InlineData(@"Valid\samplefeed7\samplefeed7.xml", @"Valid\samplefeed7\samplefeed7.header.xml")]
        [InlineData(@"Valid\samplefeed8\samplefeed8.xml", @"Valid\samplefeed8\samplefeed8.header.xml")]
        public async Task ReadHeader_Valid_ReturnsHeader(string inputFilename, string expectedFilename)
        {
            var input = File.ReadAllText($@"{TestDataRoot}{inputFilename}");
            var expected = File.ReadAllText($@"{TestDataRoot}{expectedFilename}");

            string? header;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var feedReader = new FeedReader(stream))
            {
                header = await feedReader.ReadHeader();
            }

            header.Should().Be(expected);
        }

        [Theory]
        [InlineData(@"Valid\samplefeed1\samplefeed1.xml", @"Valid\samplefeed1\samplefeed1.show.xml")]
        [InlineData(@"Valid\samplefeed2\samplefeed2.xml", @"Valid\samplefeed2\samplefeed2.show.xml")]
        [InlineData(@"Valid\samplefeed3\samplefeed3.xml", @"Valid\samplefeed3\samplefeed3.show.xml")]
        [InlineData(@"Valid\samplefeed4\samplefeed4.xml", @"Valid\samplefeed4\samplefeed4.show.xml")]
        [InlineData(@"Valid\samplefeed5\samplefeed5.xml", @"Valid\samplefeed5\samplefeed5.show.xml")]
        [InlineData(@"Valid\samplefeed7\samplefeed7.xml", @"Valid\samplefeed7\samplefeed7.show.xml")]
        [InlineData(@"Valid\samplefeed8\samplefeed8.xml", @"Valid\samplefeed8\samplefeed8.show.xml")]
        public async Task ReadShow_Valid_ReturnsShow(string inputFilename, string expectedFilename)
        {
            var input = File.ReadAllText($@"{TestDataRoot}{inputFilename}");
            var expected = File.ReadAllText($@"{TestDataRoot}{expectedFilename}");

            string? show;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var feedReader = new FeedReader(stream))
            {
                await feedReader.ReadHeader();
                show = await feedReader.ReadShow();
            }

            show.Should().Be(expected);
        }


        /*
        [Fact]
        public async Task SkipPreheader_Empty_Throws()
        {
            var input = "";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageOnly_Throws()
        {
            var input = "".PadRight(12, 'z');

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_Garbage_DoesNotThrow()
        {
            var input = "".PadRight(12, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageWithExpectedLength_DoesNotThrow()
        {
            var input = "".PadRight(12, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_GarbageLong_Throws()
        {
            var input = "".PadRight(10000, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_Html_Throws()
        {
            var input = "<html>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsXml_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsRss_DoesNotThrow()
        {
            var input = "<rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task SkipPreheader_StartsXmlWithExpectedLength_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                Func<Task> act = async () => await feedReader.SkipPreheader();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_ValidChannel_DoesNotThrow()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                Action act = () => feedReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_GarbageStartValidChannel_DoesNotThrow()
        {
            var input = "".PadRight(3, 'z') + "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                Action act = () => feedReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_ValidChannelNonXml_DoesNotThrow()
        {
            var input = "<?xml ￾version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<channel>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                Action act = () => feedReader.ReadDocumentHeader();
                act.Should().NotThrow<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task ReadDocumentMetadata_InvalidChannel_Throws()
        {
            var input = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"\r\n\txmlns:content=\"http://purl.org/rss/1.0/modules/content/\"\r\n\txmlns:wfw=\"http://wellformedweb.org/CommentAPI/\"\r\n\txmlns:dc=\"http://purl.org/dc/elements/1.1/\"\r\n\txmlns:atom=\"http://www.w3.org/2005/Atom\"\r\n\txmlns:sy=\"http://purl.org/rss/1.0/modules/syndication/\"\r\n\txmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\"\r\n\txmlns:georss=\"http://www.georss.org/georss\" xmlns:geo=\"http://www.w3.org/2003/01/geo/wgs84_pos#\" \r\n\txmlns:itunes=\"http://www.itunes.com/dtds/podcast-1.0.dtd\"\r\nxmlns:media=\"http://search.yahoo.com/mrss/\"\r\n\t>\r\n\r\n<flannel>";

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                Action act = () => feedReader.ReadDocumentHeader();
                act.Should().Throw<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_PreChannelGarbage_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_prechannelgarbage.xml");

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_TwoBufferContent_DoesNotThrow()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_twobuffercontent.xml");

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
                await act.Should().NotThrowAsync<InvalidPodcastFeedException>();
            }
        }

        [Fact]
        public async Task GetShowAsync_TooLongContent_Throws()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_toolongcontent.xml");

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                Func<Task> act = async () => await feedReader.GetShowXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                await feedReader.GetShowXmlAsync();
                var xml = await feedReader.GetNextEpisodeXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                await feedReader.GetShowXmlAsync();
                var xml = await feedReader.GetNextEpisodeXmlAsync();
                xml.Should().NotBeNull();
                xml = await feedReader.GetNextEpisodeXmlAsync();
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
                var feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                await feedReader.GetShowXmlAsync();
                await feedReader.GetNextEpisodeXmlAsync();
                var xml = await feedReader.GetNextEpisodeXmlAsync();
                xml.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetNextEpisodeAsync_TooLongContent_Throws()
        {
            var input = File.ReadAllText($@"{TestDataRoot}Invalid\samplefeed_toolongcontent2.xml");

            FeedReader feedReader;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            using (var reader = new StreamReader(stream))
            {
                feedReader = new FeedReader(reader, _logger);
                await feedReader.SkipPreheader();
                feedReader.ReadDocumentHeader();
                await feedReader.GetShowXmlAsync();
                Func<Task> act = async () => await feedReader.GetNextEpisodeXmlAsync();
                await act.Should().ThrowAsync<InvalidPodcastFeedException>();
            }
        }
        */
    }
}
