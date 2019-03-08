using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace PodcastFeedReader.Exceptions
{
    public class InvalidPropertiesException : Exception
    {
        public ImmutableDictionary<string, string> InvalidProperties { get; set; }

        public InvalidPropertiesException(IDictionary<string, string> invalidProperties)
        {
            InvalidProperties = invalidProperties.ToImmutableDictionary();
        }

        public override string ToString()
        {
            var str = $"Invalid properties: {String.Join(", ", InvalidProperties.Select(x => $"{x.Key}: {x.Value}"))}. {base.ToString()}";
            return str;
        }
    }
}
