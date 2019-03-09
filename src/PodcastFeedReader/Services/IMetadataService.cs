using System;
using System.Collections.Generic;

namespace PodcastFeedReader.Services
{
    public interface IMetadataService
    {
        ICollection<string> GetPropertyNamesMissingValueByAttribute<TEntity, TAttribute>(TEntity entity)
            where TEntity : class
            where TAttribute : Attribute;

        void Initialise(ICollection<Type> types, ICollection<Attribute> attributes);
    }
}
