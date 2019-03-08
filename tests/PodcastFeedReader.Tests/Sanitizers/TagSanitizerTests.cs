namespace PodcastFeedReader.Tests.Sanitizers
{
    public class TagSanitizerTests
    {
        /*
        [Fact]
        public void MakeValid_StartsHashtag_ReturnsWithoutHashtag()
        {
            const string arranged = "#test";
            var acted = Tag.MakeValid(arranged);
            var asserted = "test";
            Assert.Equal(asserted, acted);
        }

        [Fact]
        public void MakeValid_MultipleHashTags_ReturnsWithoutHashtags()
        {
            const string arranged = "#test#test2";
            var acted = Tag.MakeValid(arranged);
            var asserted = "test-test2";
            Assert.Equal(asserted, acted);
        }

        [Fact]
        public void MakeValid_UpperCase_ReturnsLower()
        {
            const string arranged = "tEsT";
            var acted = Tag.MakeValid(arranged);
            var asserted = "test";
            Assert.Equal(asserted, acted);
        }

        [Fact]
        public void MakeValid_Empty_ThrowsException()
        {
            const string arranged = "";
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Tag.MakeValid(arranged);
            });
        }
        */
    }
}