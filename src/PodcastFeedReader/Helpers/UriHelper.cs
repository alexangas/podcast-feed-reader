using System;
using System.Net;

namespace PodcastFeedReader.Helpers
{
    public static class UriHelper
    {
        public static string GetAbsoluteUri(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return uri.AbsoluteUri;

            url = WebUtility.UrlDecode(url);
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return uri.AbsoluteUri;

            return null;
        }
    }
}
