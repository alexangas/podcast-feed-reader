﻿using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PodcastFeedReader.Helpers;

namespace PodcastFeedReader.Readers
{
    public class FeedReader : IDisposable
    {
        private const string HeaderStartString = "<?xml";
        private const string ShowStartString = "<channel>";
        private const string ShowEndString = "</channel>";
        private const string EpisodeStartString = "<item>";
        private const string CDataStartString = "<![CDATA[";
        private const string CDataEndString = "]]>";

        private readonly PipeReader _pipeReader;

        private string? _header;

        public FeedReader(Stream stream)
        {
            _pipeReader = PipeReader.Create(stream);
            _header = null;
        }

        public async Task<string?> ReadHeader(CancellationToken cancellationToken = default)
        {
            var headerBuilder = new StringBuilder();

            while (true)
            {
                var result = await _pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;
                
                while (TryParseLine(ref buffer, out var line))
                {
                    if (headerBuilder.Length == 0)
                    {
                        var headerStart = SequenceExtensions.IndexOf(line, HeaderStartString);

                        if (headerStart != null)
                        {
                            var showStartOnHeaderStartLine = SequenceExtensions.IndexOf(line, ShowStartString);
                            var lineLength = showStartOnHeaderStartLine == null
                                ? line.Length - headerStart.Value.GetInteger()
                                : (line.Length - headerStart.Value.GetInteger()) - (line.Length - showStartOnHeaderStartLine.Value.GetInteger());
                            var restOfLineSequence = line.Slice(headerStart.Value, lineLength);
                            var restOfLineString = Encoding.UTF8.GetString(restOfLineSequence);
                            headerBuilder.AppendLine(restOfLineString);

                            if (showStartOnHeaderStartLine != null)
                            {
                                var matchPosition = showStartOnHeaderStartLine.Value;
                                _pipeReader.AdvanceTo(buffer.Start, new SequencePosition(matchPosition.GetObject(), matchPosition.GetInteger() + 1));
                                _header = headerBuilder.ToString();
                                return _header;
                            }
                        }
                    }
                    else
                    {
                        var showStart = SequenceExtensions.IndexOf(line, ShowStartString);
                        if (showStart == null)
                        {
                            var restOfLineString = Encoding.UTF8.GetString(line);
                            var cleanedUpString = restOfLineString.Trim();
                            if (cleanedUpString.Length > 0)
                                headerBuilder.AppendLine(cleanedUpString);
                        }
                        else
                        {
                            _pipeReader.AdvanceTo(buffer.Start, showStart.Value);
                            _header = headerBuilder.ToString();
                            return _header;
                        }
                    }
                }

                if (result.IsCompleted)
                {
                    if (buffer.Length > 0)
                        throw new InvalidDataException("Incomplete message.");

                    break;
                }

                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
            }

            return null;
        }

        public async Task<string?> ReadShow(CancellationToken cancellationToken = default)
        {
            var showBuilder = new StringBuilder();
            var inCData = false;

            while (true)
            {
                var result = await _pipeReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                while (TryParseLine(ref buffer, out var line))
                {
                    var episodeStart = SequenceExtensions.IndexOf(line, EpisodeStartString);
                    if (episodeStart == null)
                    {
                        var restOfLineString = Encoding.UTF8.GetString(line);
                        if (!inCData)
                        {
                            var cleanedUpString = restOfLineString.Trim();
                            if (cleanedUpString.Length > 0)
                                showBuilder.AppendLine(cleanedUpString);
                        }
                        else
                        {
                            showBuilder.AppendLine(restOfLineString);
                        }

                        if (!inCData)
                        {
                            var foundCDataStart = SequenceExtensions.IndexOf(line, CDataStartString);
                            if (foundCDataStart != null)
                                inCData = true;
                        }
                        if (inCData)
                        {
                            var foundCDataEnd = SequenceExtensions.IndexOf(line, CDataEndString);   // TODO: Add start position parameter
                            if (foundCDataEnd != null)
                                inCData = false;
                        }
                    }
                    else
                    {
                        _pipeReader.AdvanceTo(buffer.Start, episodeStart.Value);
                        showBuilder.AppendLine(ShowEndString);
                        var show = showBuilder.ToString();
                        return show;
                    }
                }

                if (result.IsCompleted)
                {
                    if (buffer.Length > 0)
                        throw new InvalidDataException("Incomplete message.");

                    break;
                }

                _pipeReader.AdvanceTo(buffer.Start, buffer.End);
            }

            return null;
        }

        private static bool TryParseLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            var reader = new SequenceReader<byte>(buffer);
            if (reader.TryReadToAny(out line, NewLineDelimiters, advancePastDelimiter: false))
            {
                reader.IsNext((byte) '\r', advancePast: true);
                reader.IsNext((byte) '\n', advancePast: true);

                buffer = buffer.Slice(reader.Position);
                return true;
            }

            line = default;
            return false;
        }

        private static ReadOnlySpan<byte> NewLineDelimiters => new[] {(byte) '\r', (byte) '\n'};

        /*
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
            _streamProcessedIndex = _stringBuffer.IndexOf("<item", StringComparison.Ordinal);

            _contentBuilder = new StringBuilder(_bufferBuilder.Capacity);
            ProcessXml(_bufferBuilder, _contentBuilder, isShow: true);

            var indexOfItem = _contentBuilder.IndexOf("<item");
            var showSubstring = _contentBuilder.ToString(0, indexOfItem);
            var showContent = $"{_header}{showSubstring}</channel></rss>";
            var showXml = XmlHelper.ReadXml(showContent);
            
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

                _posEpisodeItemEndIndex = _stringBuffer.IndexOf("</item>", StringComparison.Ordinal);
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
            var episodeXml = XmlHelper.ReadXml(episodeContent);
            _streamProcessedIndex = -1;

            return episodeXml;
        }

        private async Task<int> ReadFromStream()
        {
            Array.Clear(_streamBuffer, 0, _streamBuffer.Length);
            _logger.LogTrace("Reading from stream");
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
        */

        public void Dispose()
        {
            _pipeReader.Complete();
        }
    }
}
