using System;
using System.Buffers;
using System.Text;
using FluentAssertions;
using PodcastFeedReader.Helpers;
using Xunit;

namespace PodcastFeedReader.Tests.Helpers
{
    public class SequenceExtensionsTests
    {
        [Theory]
        [InlineData("abc", "a", 0)]
        [InlineData("abc", "aaaaa", 0)]
        [InlineData("abc", "b", 1)]
        [InlineData("abc", "c", 2)]
        [InlineData("abc", "A", 0)]
        [InlineData("abc", "Aaaaa", 0)]
        [InlineData("abc", "B", 1)]
        [InlineData("Abc", "a", 0)]
        [InlineData("Abc", "aaaaa", 0)]
        [InlineData("aBc", "b", 1)]
        [InlineData("abc", "z", null)]
        [InlineData("abc", "zzzzz", null)]
        [InlineData("abc", "def", null)]
        [InlineData("abc", "bc", 1)]
        [InlineData("abcd", "bc", 1)]
        [InlineData("abcdbce", "bc", 1)]
        [InlineData("abcdef", "e", 4)]
        [InlineData("abc", "abcd", null)]
        public void IndexOf_Simple(string input, string match, int? expected)
        {
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(input));

            var result = SequenceExtensions.IndexOf(sequence, match, StringComparison.OrdinalIgnoreCase);

            result?.GetInteger().Should().Be(expected);
        }

        [Trait("Category", "Performance")]
        [Theory]
        [InlineData(8, 8)]
        [InlineData(8, 16)]
        [InlineData(64, 12)]
        [InlineData(128, 12)]
        [InlineData(256, 12)]
        [InlineData(512, 12)]
        [InlineData(1024, 12)]
        [InlineData(2048, 12)]
        [InlineData(4096, 12)]
        [InlineData(8192, 12)]
        [InlineData(16384, 12)]
        [InlineData(32768, 12)]
        public void IndexOf_Performance_OrdinalIgnoreCase(int inputLength, int matchLength)
        {
            var input = new string('a', inputLength);
            var match = new string('b', matchLength);
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(input + match));

            var result = SequenceExtensions.IndexOf(sequence, match, stringComparison: StringComparison.OrdinalIgnoreCase);

            result.Should().NotBeNull();
        }

        [Trait("Category", "Performance")]
        [Theory]
        [InlineData(8, 8)]
        [InlineData(8, 16)]
        [InlineData(64, 12)]
        [InlineData(128, 12)]
        [InlineData(256, 12)]
        [InlineData(512, 12)]
        [InlineData(1024, 12)]
        [InlineData(2048, 12)]
        [InlineData(4096, 12)]
        [InlineData(8192, 12)]
        [InlineData(16384, 12)]
        [InlineData(32768, 12)]
        public void IndexOf_Performance_Ordinal(int inputLength, int matchLength)
        {
            var input = new string('a', inputLength);
            var match = new string('b', matchLength);
            var sequence = new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(input + match));

            var result = SequenceExtensions.IndexOf(sequence, match, stringComparison: StringComparison.Ordinal);

            result.Should().NotBeNull();
        }
    }
}
