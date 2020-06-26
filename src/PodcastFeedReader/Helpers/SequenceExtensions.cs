using System;
using System.Buffers;

namespace PodcastFeedReader.Helpers
{
    public static class SequenceExtensions
    {
        public static SequencePosition? IndexOf(in ReadOnlySequence<byte> buffer, string str, StringComparison stringComparison = StringComparison.Ordinal)
        {
            var reader = new SequenceReader<byte>(buffer);
            var strMatchIndex = 0;
            SequencePosition? matchStartPosition = null;

            if (stringComparison == StringComparison.Ordinal)
            {
                if (!reader.TryAdvanceTo((byte) str[0], advancePastDelimiter: false))
                    return null;
            }

            while (!reader.End)
            {
                SequencePosition? preReadPosition = null;
                if (matchStartPosition == null)
                    preReadPosition = reader.Position;

                if (!reader.TryRead(out var bufferByte))
                    return null;
                var bufferSpan = new ReadOnlySpan<char>(new[] {(char) bufferByte});
                var strSpan = str.AsSpan(strMatchIndex, 1);

                var match = bufferSpan.Equals(strSpan, stringComparison);

                if (match)
                {
                    if (matchStartPosition == null)
                        matchStartPosition = preReadPosition;
                    strMatchIndex++;

                    if (strMatchIndex == str.Length)
                        return matchStartPosition;
                }
                else
                {
                    matchStartPosition = null;
                    strMatchIndex = 0;
                }
            }

            return null;
        }
    }
}