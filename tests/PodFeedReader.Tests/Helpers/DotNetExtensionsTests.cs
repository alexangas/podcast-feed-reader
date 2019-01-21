using System.Text;
using PodFeedReader.Helpers;
using Xunit;

namespace PodFeedReader.Tests.Helpers
{
    public class DotNetExtensionsTests
    {
        [Fact]
        public void LastIndexOf_Empty_ReturnsNotFound()
        {
            var builder = new StringBuilder();
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(-1, index);
        }

        [Fact]
        public void LastIndexOf_NoMatch_ReturnsNotFound()
        {
            var builder = new StringBuilder("0");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(-1, index);
        }

        [Fact]
        public void LastIndexOf_OneMatch_ReturnsOneMatch()
        {
            var builder = new StringBuilder("a");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(0, index);
        }

        [Fact]
        public void LastIndexOf_OneMatchAgain_ReturnsOneMatch()
        {
            var builder = new StringBuilder("ab");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(0, index);
        }

        [Fact]
        public void LastIndexOf_OneMatchAnother_ReturnsOneMatch()
        {
            var builder = new StringBuilder("ba");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(1, index);
        }

        [Fact]
        public void LastIndexOf_TwoMatches_ReturnsLastMatch()
        {
            var builder = new StringBuilder("aba");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(2, index);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesAgain_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abab");
            var substring = "a";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(2, index);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesWord_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abcd_bc_ef");
            var substring = "bc";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(5, index);
        }

        [Fact]
        public void LastIndexOf_TwoMatchesWordPartial_ReturnsLastMatch()
        {
            var builder = new StringBuilder("abcd_bac_ef");
            var substring = "bc";

            var index = builder.LastIndexOf(substring);

            Assert.Equal(1, index);
        }
   }
}