using System;
using System.Text.RegularExpressions;

namespace PodcastFeedReader.Sanitizers
{
    public class TagSanitizer
    {
        private static readonly Regex NoWordCharsRegex = new Regex(@"\W");
        private static readonly Regex MultipleDashCharsRegex = new Regex(@"\-{2,}");
        private static readonly Regex StartOrEndDashCharsRegex = new Regex(@"^\-|\-$");

        public static string MakeValid(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentOutOfRangeException(nameof(name));

            var toLower = name.ToLowerInvariant();
            var nonWordCharsReplaced = NoWordCharsRegex.Replace(toLower, "-");
            var multipleDashesReplaced = MultipleDashCharsRegex.Replace(nonWordCharsReplaced, "-");
            var startOrEndDashesReplaced = StartOrEndDashCharsRegex.Replace(multipleDashesReplaced, "");
            var valid = startOrEndDashesReplaced;
            return valid;
        }
    }
}
