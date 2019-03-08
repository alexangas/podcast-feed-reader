namespace PodcastFeedReader.Tests.Sanitizers
{
    public class ShowSanitizerTests
    {
        /*
        [Fact]
        public void ParseFromXml_WithNoContents_Throws()
        {
            var doc = XDocument.Parse("<xml></xml>").Root;

            ICollection<string> missingProperties;
            Assert.Throws<InvalidPropertiesException>(() =>
            {
                var parser = new ShowParser(_sanitizationService);
                parser.ParseFromXml(doc);
                parser.GetEntityForDisplay(out missingProperties);
            });
        }

        [Fact]
        public void ParseFromXml_WithContents_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed1.xml");
            var doc = XDocument.Parse(text).Root;

            ICollection<string> missingProperties;
            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.Equal("#JustSayin", show.Title);
            //Assert.Equal("https://justsayinpodcast.wordpress.com/category/comedy/feed/", show.FeedLink);
            Assert.Equal("https://justsayinpodcast.wordpress.com/", show.WebLink);
            Assert.Equal("http://justsayinpodcast.files.wordpress.com/2015/07/cropped-podcast-logo.jpg", show.ImageLink);
            Assert.Equal("A Comedy Podcast", show.Subtitle);
            Assert.Equal("#justSayin is a random comedy podcast, with Boston accents and a variety of subjects.", show.Description);
            Assert.Equal("Ryan &amp; Tara", show.Author);
            Assert.Equal("en", show.Language);
            Assert.Equal(new List<string>(new[] {"comedy", "entertainment", "boston", "massachusetts", "random", "arts", "performing-arts", "games-hobbies", "video-games" }), show.Tags);

            Assert.Empty(missingProperties);
        }

        [Fact]
        public void ParseFromXml_Author_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_authormissing.xml");
            var doc = XDocument.Parse(text).Root;

            ICollection<string> missingProperties;
            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.NotEmpty(show.Author);
        }

        [Fact]
        public void ParseFromXml_Subtitle_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_subtitlemissing.xml");
            var doc = XDocument.Parse(text).Root;

            ICollection<string> missingProperties;
            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.NotEmpty(show.Subtitle);
        }

        [Fact]
        public void ParseFromXml_Description_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_descriptionmissing.xml");
            var doc = XDocument.Parse(text).Root;

            ICollection<string> missingProperties;
            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.NotEmpty(show.Description);
        }

        [Fact]
        public void ParseFromXml_Image_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_imagemissing.xml");
            var doc = XDocument.Parse(text).Root;

            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            ICollection<string> missingProperties;
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.NotEmpty(show.ImageLink);
        }

        [Fact]
        public void ParseFromXml_Tags_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_tagsmissing.xml");
            var doc = XDocument.Parse(text).Root;

            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            ICollection<string> missingProperties;
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.NotEmpty(show.Tags);
            Assert.Equal(3, show.Tags.Count);
            Assert.Equal(new List<string>(new[] { "itunes-u", "business", "finance" }), show.Tags);
        }

        [Fact]
        public void ParseFromXml_UrlEncoded_ReturnsExpected()
        {
            var text = File.ReadAllText("SampleData\\samplefeed_videoandencodedlink.xml");
            var doc = XDocument.Parse(text).Root;

            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            ICollection<string> missingProperties;
            var show = parser.GetEntityForDisplay(out missingProperties);

            Assert.Equal("http://revision3.com/tekzilla", show.WebLink);
        }

        [Fact]
        public void ParseFromXmlSearchText_DescriptionMatchesSubtitle_ReturnsExpected()
        {
            var text = @"<rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:content=""http://purl.org/rss/1.0/modules/content/"" xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule"" xmlns:media=""http://search.yahoo.com/mrss/"" xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:ddn=""http://discoverydn.com/about"">
    <channel>
        <title>Tekzilla</title>
        <description>All Tekzilla, all the time</description>
        <itunes:summary>All Tekzilla, all the time</itunes:summary>
        <itunes:subtitle>All Tekzilla, all the time</itunes:subtitle>
</channel>
</rss>
";
            var doc = XDocument.Parse(text).Root;

            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForSearch(new EntityKey());

            Assert.Equal("Tekzilla", show.Title);
            Assert.Equal("All Tekzilla, all the time", show.Subtitle);
            Assert.Equal(null, show.Description);
            Assert.Equal(ShowSearchMatching.DescriptionEqualsSubtitle, show.Matching);
        }

        [Fact]
        public void ParseFromXmlSearchText_AllDistinct_ReturnsExpected()
        {
            var text = @"<rss version=""2.0"" xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:content=""http://purl.org/rss/1.0/modules/content/"" xmlns:creativeCommons=""http://backend.userland.com/creativeCommonsRssModule"" xmlns:media=""http://search.yahoo.com/mrss/"" xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:ddn=""http://discoverydn.com/about"">
    <channel>
        <title>Tekzilla</title>
        <description>All Tekzilla, all the time</description>
        <itunes:summary>ABC0</itunes:summary>
        <itunes:subtitle>ABC1</itunes:subtitle>
</channel>
</rss>
";
            var doc = XDocument.Parse(text).Root;

            var parser = new ShowParser(_sanitizationService);
            parser.ParseFromXml(doc);
            var show = parser.GetEntityForSearch(new EntityKey());

            Assert.Equal("Tekzilla", show.Title);
            Assert.Equal("All Tekzilla, all the time", show.Subtitle);
            Assert.Equal("ABC0", show.Description);
            Assert.Equal(ShowSearchMatching.AllDistinct, show.Matching);
        }
        */
    }
}