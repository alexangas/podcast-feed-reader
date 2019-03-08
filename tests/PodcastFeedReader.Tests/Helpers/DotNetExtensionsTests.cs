using System;
using System.Text;
using FluentAssertions;
using PodcastFeedReader.Helpers;
using Xunit;

namespace PodcastFeedReader.Tests.Helpers
{
    public class DotNetExtensionsTests
    {
        [Fact]
        public void LastIndexOf_Empty_ReturnsNotFound()
        {
            var builder = new StringBuilder();
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(-1);
        }

        [Fact]
        public void LastIndexOf_NoMatch_ReturnsNotFound()
        {
            var builder = new StringBuilder("0");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(-1);
        }

        [Fact]
        public void LastIndexOf_OneMatch_ReturnsOneMatch()
        {
            var builder = new StringBuilder("a");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(0);
        }

        [Fact]
        public void LastIndexOf_OneMatchAgain_ReturnsOneMatch()
        {
            var builder = new StringBuilder("ab");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(0);
        }

        [Fact]
        public void LastIndexOf_OneMatchAnother_ReturnsOneMatch()
        {
            var builder = new StringBuilder("ba");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(1);
        }

        [Fact]
        public void LastIndexOf_TwoMatches_ReturnsLastMatch()
        {
            var builder = new StringBuilder("aba");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(2);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesAgain_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abab");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(2);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesWord_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abcd_bc_ef");
            var substring = "bc";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(5);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesWordPartial_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abcd_bac_ef");
            var substring = "bc";

            var index = builder.LastIndexOf(substring);

            index.Should().Be(1);
        }

        [Trait("Category", "Performance")]
        [Fact]
        public static void IndexOfPerfTest()
        {
            var rnd = new Random();
            StringBuilder s = new StringBuilder();
            StringBuilder s2 = new StringBuilder();
            for (var x = 0; x < 500000; x++)
            {
                s.Clear();
                s.Append(rnd.Next(Int32.MinValue, Int32.MaxValue).ToString()).Append('*', 1024 * 1024);
                s2.Clear();
                s2.Append(rnd.Next(Int32.MinValue, Int32.MaxValue).ToString()).Append('-', 1024);
                int r;
                if (x % 2 == 0)
                {
                    r = s.IndexOf(s2.ToString());
                }
                else
                {
                    r = s.IndexOf(s.ToString());
                }
                if (x % 3 == 0)
                {
                    r = s.IndexOf(s2.ToString(), startPos: rnd.Next(1, 1024));
                }
                System.Diagnostics.Debug.WriteLine(r);
            }
        }
    }
}
