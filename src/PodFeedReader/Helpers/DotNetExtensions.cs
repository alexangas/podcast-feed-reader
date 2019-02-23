using System.Runtime.CompilerServices;
using System.Text;

namespace PodFeedReader.Helpers
{
    public static class DotNetExtensions
    {
        // From http://stackoverflow.com/a/19361102/6651
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this StringBuilder builder, string substring, int startPos = 0)
        {
            const int notFound = -1;
            var builderLength = builder.Length;
            var substringLength = substring.Length;
            if (builderLength < substringLength)
                return notFound;

            var loopEnd = builderLength - substringLength + 1;
            for (var loopIndex = startPos; loopIndex < loopEnd; loopIndex++)
            {
                var found = true;
                for (var innerLoop = 0; innerLoop < substringLength; innerLoop++)
                {
                    var builderChar = builder[loopIndex + innerLoop];
                    var substringChar = substring[innerLoop];
                    //if (caseInsensitive)
                    //{
                    //    builderChar = builderChar.ToLowerFast();
                    //    substringChar = substringChar.ToLowerFast();
                    //}
                    if (builderChar == substringChar)
                        continue;
                    found = false;
                    break;
                }
                if (found)
                    return loopIndex;
            }
            return notFound;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastIndexOf(this StringBuilder builder, string substring)
        {
            var lastFoundIndex = -1;
            for(;;)
            {
                var startPos = lastFoundIndex == -1 ? 0 : lastFoundIndex + substring.Length;
                var foundIndex = builder.IndexOf(substring, startPos: startPos);
                if (foundIndex == -1)
                    return lastFoundIndex;
                lastFoundIndex = foundIndex;
            }
        }
    }
}
