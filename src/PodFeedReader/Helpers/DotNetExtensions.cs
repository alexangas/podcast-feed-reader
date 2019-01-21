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

        //public static void IndexOfPerfTest()
        //{
        //    var rnd = new Random();
        //    StringBuilder s = new StringBuilder();
        //    StringBuilder s2 = new StringBuilder();
        //    for (var x = 0; x < 500000; x++)
        //    {
        //        s.Clear();
        //        s.Append(rnd.Next(Int32.MinValue, Int32.MaxValue).ToString()).Append('*', 1024 * 1024);
        //        s2.Clear();
        //        s2.Append(rnd.Next(Int32.MinValue, Int32.MaxValue).ToString()).Append('-', 1024);
        //        int r;
        //        if (x % 2 == 0)
        //        {
        //            r = IndexOf(s, s2.ToString());
        //        }
        //        else
        //        {
        //            r = IndexOf(s, s.ToString());
        //        }
        //        if (x % 3 == 0)
        //        {
        //            r = IndexOf(s, s2.ToString(), startPos: rnd.Next(1, 1024));
        //        }
        //        System.Diagnostics.Debug.WriteLine(r);
        //    }
        //}

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

        //public static int IndexOf(this StringBuilder builder, string[] substrings, bool caseInsensitive = false)
        //{
        //    foreach (var value in substrings)
        //    {
        //        var position = builder.IndexOf(value);
        //        if (position >= 0)
        //            return position;
        //    }
        //    return -1;
        //}
    }
}
