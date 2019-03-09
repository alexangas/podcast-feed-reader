namespace PodcastFeedReader.Tests.Parsers
{
    public class BaseParserTests
    {
        /*
        [Fact]
        public void PreParseCleanUp_WithStartAndEndTag_RemovesStartAndEndTag()
        {
            const string testString = @"<powerpress>\n\nA Classic Loveline episode with Adam and Dr. Drew featuring guest Angelica Bridges. They talk about nude sunbating and Estie Kitchen fire stories. (Original airdate: October 11, 2011)\n\nFor more Classic Loveline, visit http://www.podcastone.com/Classic-Loveline</powerpress>";

            var result = ParserHelper.PreParseCleanUp(testString);

            const string expectedString = @"\n\nA Classic Loveline episode with Adam and Dr. Drew featuring guest Angelica Bridges. They talk about nude sunbating and Estie Kitchen fire stories. (Original airdate: October 11, 2011)\n\nFor more Classic Loveline, visit http://www.podcastone.com/Classic-Loveline";
            Assert.Equal(expectedString, result);
        }

        [Fact]
        public void PreParseCleanUp_WithNoTags_RemovesNothing()
        {
            const string testString = @"\n\nA Classic Loveline episode with Adam and Dr. Drew featuring guest Angelica Bridges. They talk about nude sunbating and Estie Kitchen fire stories. (Original airdate: October 11, 2011)\n\nFor more Classic Loveline, visit http://www.podcastone.com/Classic-Loveline";

            var result = ParserHelper.PreParseCleanUp(testString);

            const string expectedString = @"\n\nA Classic Loveline episode with Adam and Dr. Drew featuring guest Angelica Bridges. They talk about nude sunbating and Estie Kitchen fire stories. (Original airdate: October 11, 2011)\n\nFor more Classic Loveline, visit http://www.podcastone.com/Classic-Loveline";
            Assert.Equal(expectedString, result);
        }

        [Fact]
        public void PreParseCleanUp_WithSelfEnclosedTag_RemovesEnclosedTag()
        {
            const string testString = @"<img src=""http://feeds.feedburner.com/~r/ElevationChurchCharlotte/~4/kdleKUXQJXk"" height=""1"" width=""1"" alt=""""/>";

            var result = ParserHelper.PreParseCleanUp(testString);

            const string expectedString = "";
            Assert.Equal(expectedString, result);
        }         */
    }
}