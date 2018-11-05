using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using PodApp.Data.Collection.Helpers;
using PodApp.Data.Collection.Helpers.Xml;
using PodApp.Data.Model.Helpers;
using PodApp.Data.Model.Services;

namespace PodApp.Data.Collection.Readers
{
    public class PodcastFeedReader
    {
        private const short BufferSize = 4096;
        private const int MaxShowLength = 8192;
        private const int MaxEpisodeLength = 128 * 1024;

        private static readonly string[] FeedStartStrings = { "<?xml", "<rss", "<feed" };

        private readonly StreamReader _baseReader;
        private readonly ILogger<PodcastFeedReader> _logger;
        private readonly char[] _streamBuffer;
        private StringBuilder _bufferBuilder;
        private StringBuilder _contentBuilder;

        private int _streamProcessedIndex;
        private int _posEpisodeItemEndIndex;
        private string _stringBuffer;
        private string _header;

        public PodcastFeedReader(StreamReader baseReader, ILogger<PodcastFeedReader> logger)
        {
            if (baseReader == null)
                throw new ArgumentNullException(nameof(baseReader));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _baseReader = baseReader;
            _logger = logger;

            _streamBuffer = new char[BufferSize];

            _posEpisodeItemEndIndex = -1;
        }

        public async Task SkipPreheader()
        {
            if (_baseReader.EndOfStream)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.UnexpectedEndOfStream);

            var bytesRead = await _baseReader.ReadBlockAsync(_streamBuffer, 0, BufferSize);
            if (bytesRead <= 0)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.UnexpectedByteCount);
            _streamProcessedIndex = -1;
            _stringBuffer = new string(_streamBuffer, 0, bytesRead);
            
            if (_stringBuffer.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.HtmlDocument, _stringBuffer);

            foreach (var feedStartString in FeedStartStrings)
            {
                _streamProcessedIndex = _stringBuffer.IndexOf(feedStartString, StringComparison.OrdinalIgnoreCase);
                if (_streamProcessedIndex >= 0)
                    break;
            }
            if (_streamProcessedIndex < 0)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.FeedStartNotFound, _stringBuffer);
        }

        public void ReadDocumentHeader()
        {
            var posChannelStart = _stringBuffer.IndexOf("<channel", StringComparison.OrdinalIgnoreCase);
            if (posChannelStart <= _streamProcessedIndex)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.ShowStartNotFound, _stringBuffer);

            _bufferBuilder = new StringBuilder(posChannelStart - _streamProcessedIndex);

            for (var chIndex = _streamProcessedIndex; chIndex < posChannelStart; chIndex++)
            {
                var ch = _stringBuffer[chIndex];
                if (!XmlConvert.IsXmlChar(ch))
                    continue;
                _bufferBuilder.Append(ch);
            }

            _header = _bufferBuilder.ToString().Trim();
            _streamProcessedIndex = posChannelStart;
        }

        public async Task<XDocument> GetShowXmlAsync()
        {
            int posEpisodeItemStart;
            _stringBuffer = _stringBuffer.Substring(_streamProcessedIndex);
            var totalBytesRead = 0;
            _bufferBuilder.Capacity = Math.Max(_bufferBuilder.Capacity, BufferSize);
            _bufferBuilder.Clear();

            for (;;)
            {
                _bufferBuilder.Append(_stringBuffer);

                posEpisodeItemStart = _bufferBuilder.IndexOf("<item");
                if (posEpisodeItemStart >= 0)
                    break;

                if (_baseReader.EndOfStream)
                    break;

                if (!_baseReader.EndOfStream)
                {
                    var bytesRead = await ReadFromStream();
                    totalBytesRead += bytesRead;
                    _streamProcessedIndex += _stringBuffer.Length;
                }

                if (totalBytesRead > MaxShowLength)
                    throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.ShowContentTooLong, _bufferBuilder.ToString());
            }

            if (posEpisodeItemStart < 0)
                throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.EpisodeStartNotFound, _bufferBuilder.ToString());
            _streamProcessedIndex = _stringBuffer.IndexOf("<item");

            _contentBuilder = new StringBuilder(_bufferBuilder.Capacity);
            ProcessXml(_bufferBuilder, _contentBuilder, isShow: true);

            var indexOfItem = _contentBuilder.IndexOf("<item");
            var showSubstring = _contentBuilder.ToString(0, indexOfItem);
            var showContent = $"{_header}{showSubstring}</channel></rss>";
            var showXml = XmlHelper.Parse(showContent);
            
            return showXml;
        }

        public async Task<XDocument> GetNextEpisodeXmlAsync()
        {
            var stringBufferStartIndex = Math.Max(_streamProcessedIndex, _posEpisodeItemEndIndex);
            _logger.LogTrace($"Getting next episode from index {stringBufferStartIndex}");
            _stringBuffer = _stringBuffer.Substring(stringBufferStartIndex);    // Chooses first ep after show content or next ep after last ep
            var totalBytesRead = 0;
            _bufferBuilder.Capacity = Math.Max(_bufferBuilder.Capacity, BufferSize);
            _bufferBuilder.Clear();

            for (;;)
            {
                _bufferBuilder.Append(_stringBuffer);

                _posEpisodeItemEndIndex = _stringBuffer.IndexOf("</item>");
                if (_posEpisodeItemEndIndex >= 0)
                {
                    _posEpisodeItemEndIndex += 7;   //"</item>".Length;
                    break;
                }

                if (_baseReader.EndOfStream)
                    break;

                if (!_baseReader.EndOfStream)
                {
                    var bytesRead = await ReadFromStream();
                    totalBytesRead += bytesRead;
                }

                if (totalBytesRead > MaxEpisodeLength)
                    throw new InvalidPodcastFeedException(InvalidPodcastFeedException.InvalidPodcastFeedReason.EpisodeContentTooLong, _bufferBuilder.ToString());
            }

            if (_posEpisodeItemEndIndex < 0)
                return null;

            ProcessXml(_bufferBuilder, _contentBuilder, isShow: false);

            var episodeContent = $"{_header}{_contentBuilder}</rss>";
            var episodeXml = XmlHelper.Parse(episodeContent);
            _streamProcessedIndex = -1;

            return episodeXml;
        }

        private async Task<int> ReadFromStream()
        {
            Array.Clear(_streamBuffer, 0, _streamBuffer.Length);
            _logger.LogTrace($"Reading from stream");
            var bytesRead = await _baseReader.ReadBlockAsync(_streamBuffer, 0, BufferSize);
            if (bytesRead > 0)
            {
                _stringBuffer = new string(_streamBuffer, 0, bytesRead);
            }
            else
            {
                _stringBuffer = new string(_streamBuffer);
                bytesRead = BufferSize;
            }
            return bytesRead;
        }

        private static void ProcessXml(StringBuilder originalBuffer, StringBuilder cleanedBuffer, bool isShow)
        {
            cleanedBuffer.Capacity = Math.Max(originalBuffer.Capacity, cleanedBuffer.Capacity);
            cleanedBuffer.Clear();

            for (var chIndex = 0; chIndex < originalBuffer.Length; chIndex++)
            {
                var ch = originalBuffer[chIndex];

                if (!XmlConvert.IsXmlChar(ch))
                    continue;

                if (!isShow &&
                    ch == '<' && chIndex + 6 < originalBuffer.Length &&
                    originalBuffer[chIndex + 1] == '/' && originalBuffer[chIndex + 2] == 'i' && originalBuffer[chIndex + 3] == 't' && originalBuffer[chIndex + 4] == 'e' &&
                    originalBuffer[chIndex + 5] == 'm' && originalBuffer[chIndex + 6] == '>')
                {
                    // Finished ep
                    cleanedBuffer.Append("</item>");
                    return;
                }

                cleanedBuffer.Append(ch);

                // Make sure ampersands are encoded
                if (ch == '&' && chIndex + 4 < originalBuffer.Length &&
                    (originalBuffer[chIndex + 1] != 'a' || originalBuffer[chIndex + 2] != 'm' || originalBuffer[chIndex + 3] != 'p' || originalBuffer[chIndex + 4] != ';'))
                {
                    var mostRecentCDataStartIndex = cleanedBuffer.LastIndexOf("<![CDATA[");
                    if (mostRecentCDataStartIndex >= 0)
                    {
                        var mostRecentCDataEndIndex = cleanedBuffer.LastIndexOf("]]>");
                        if (mostRecentCDataEndIndex >= 0)
                        {
                            // After CDATA
                            cleanedBuffer.Append("amp;");
                        }
                    }
                    else
                    {
                        // No CDATA
                        cleanedBuffer.Append("amp;");
                    }
                }
            }
        }
    }
}
