﻿namespace PodcastFeedReader.Services
{
    public interface ISanitizationService
    {
        string SanitizeToTextOnly(string inputText);

        string SanitizeToWebDisplay(string inputText);
    }
}
