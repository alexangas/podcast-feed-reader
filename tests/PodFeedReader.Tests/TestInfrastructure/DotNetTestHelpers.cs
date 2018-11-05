using System.IO;

namespace PodApp.Tests.Model.TestHelpers
{
    public static class DotNetTestHelpers
    {
        // http://stackoverflow.com/a/1879470/6651
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}