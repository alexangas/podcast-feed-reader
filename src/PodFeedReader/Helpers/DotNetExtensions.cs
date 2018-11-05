using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PodApp.Data.Model.Helpers
{
    public static class DotNetExtensions
    {
        private const string LookupStringLowerCase = "---------------------------------!-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";
        
        private static class ThreadSafeRandom
        {
            [ThreadStatic]
            private static Random _local;

            internal static Random ThisThreadsRandom
            {
                get { return _local ?? (_local = new Random(unchecked(Environment.TickCount * 31 + Environment.CurrentManagedThreadId))); }
            }
        }

        // From http://stackoverflow.com/a/1262619/6651
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        // From http://www.dotnetperls.com/char-lowercase-optimization
        public static char ToLowerFast(this char c)
        {
            return LookupStringLowerCase[c];
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWith(this StringBuilder stringBuilder, string substring)
        {
            var builderLength = stringBuilder.Length;
            var substringLength = substring.Length;
            if (builderLength < substringLength)
                return false;

            for (var loopIndex = 0; loopIndex < substringLength; loopIndex++)
            {
                if (stringBuilder[loopIndex] != substring[loopIndex])
                    return false;
            }

            return true;
        }

        //public static void StartsWithPerfTest()
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
        //        bool r;
        //        if (x % 2 == 0)
        //        {
        //            r = StartsWith(s, s2.ToString());
        //        }
        //        else
        //        {
        //            r = StartsWith(s, s.ToString());
        //        }
        //        System.Diagnostics.Debug.WriteLine(r);
        //    }
        //}

        public static StringBuilder AppendSubstring(this StringBuilder stringBuilder, StringBuilder sourceBuilder, int length)
        {
            for (var substringIndex = 0; substringIndex < length; substringIndex++)
            {
                var ch = sourceBuilder[substringIndex];
                stringBuilder.Append(ch);
            }
            return stringBuilder;
        }

        public static char FirstChar(this StringBuilder stringBuilder)
        {
            return stringBuilder[0];
        }

        public static char PopChar(this StringBuilder stringBuilder)
        {
            var result = stringBuilder.FirstChar();
            stringBuilder.Remove(0, 1);
            return result;
        }

        public static StringBuilder AppendAndPopCharFrom(this StringBuilder stringBuilder, StringBuilder sourceBuilder)
        {
            return stringBuilder.AppendAndPopCharFrom(sourceBuilder, 1);
        }

        public static StringBuilder AppendAndPopCharFrom(this StringBuilder stringBuilder, StringBuilder sourceBuilder, int length)
        {
            stringBuilder.AppendSubstring(sourceBuilder, length);
            sourceBuilder.Remove(0, length);
            return stringBuilder;
        }

        public static string Right(this string source, int tailLength)
        {
            if (tailLength >= source.Length)
                return source;
            return source.Substring(source.Length - tailLength);
        }

        public static string SimplifyUri(Uri uri)
        {
            return $"{uri.Host}{uri.PathAndQuery}{uri.Fragment}";
        }

        public static void RetryHandlingTransientErrors(Action action, int sleepMilliseconds, int retries, params string[] transientExceptionMessages)
        {
            var isTransient = false;
            Exception transientException = null;

            for (var i = 0; i < retries; i++)
            {
                try
                {
                    action();
                    break;
                }
                catch (Exception ex)
                {
                    var exString = ex.ToString();
                    foreach (var transientExceptionMessage in transientExceptionMessages)
                    {
                        if (exString.Contains(transientExceptionMessage))
                        {
                            isTransient = true;
                            transientException = ex;
                            break;
                        }
                        isTransient = false;
                    }

                    if (isTransient)
                        Thread.Sleep(sleepMilliseconds);
                    else
                        throw;
                }
            }

            if (isTransient)
                throw transientException;
        }

        public static async Task RetryHandlingTransientErrorsAsync(Func<Task> action, int sleepMilliseconds, int retries, params string[] transientExceptionMessages)
        {
            var isTransient = false;
            Exception transientException = null;

            for (var i = 0; i < retries; i++)
            {
                try
                {
                    await action();
                    break;
                }
                catch (Exception ex)
                {
                    var exString = ex.ToString();
                    foreach (var transientExceptionMessage in transientExceptionMessages)
                    {
                        if (exString.Contains(transientExceptionMessage))
                        {
                            isTransient = true;
                            transientException = ex;
                            break;
                        }
                        isTransient = false;
                    }

                    if (isTransient)
                        Thread.Sleep(sleepMilliseconds);
                    else
                        throw;
                }
            }

            if (isTransient)
                throw transientException;
        }
    }
}
