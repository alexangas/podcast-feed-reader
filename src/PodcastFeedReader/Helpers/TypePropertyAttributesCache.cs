using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using PodcastFeedReader.Model.DataAnnotations;

namespace PodcastFeedReader.Helpers
{
    public static class TypePropertyAttributesCache
    {
        private static readonly object PropertiesForTypeLock;

        private static readonly Dictionary<Type, ICollection<PropertyInfo>> RequiredPropertiesForType;

        private static readonly Dictionary<Type, ICollection<PropertyInfo>> ExpectedPropertiesForType;

        static TypePropertyAttributesCache()
        {
            PropertiesForTypeLock = new object();
            RequiredPropertiesForType = new Dictionary<Type, ICollection<PropertyInfo>>(2);
            ExpectedPropertiesForType = new Dictionary<Type, ICollection<PropertyInfo>>(2);
        }

        public static void AddTypesToCache(params Type[] types)
        {
            lock (PropertiesForTypeLock)
            {
                foreach (var type in types)
                {
                    var requiredProperties = GetClassPropertiesByAttributeType<RequiredAttribute>(type);
                    RequiredPropertiesForType[type] = requiredProperties;

                    var expectedProperties = GetClassPropertiesByAttributeType<ExpectedAttribute>(type);
                    ExpectedPropertiesForType[type] = expectedProperties;
                }
            }

            foreach (var type in types)
            {
                var propertyTypes = RequiredPropertiesForType[type]
                    .Union(ExpectedPropertiesForType[type])
                    .Select(x => x.PropertyType)
                    .Distinct();
                DefaultValueCache.AddDefaultValuesToCache(propertyTypes);
            }
        }

        public static ICollection<string> GetRequiredProperties<T>(T entity)
        {
            var type = entity.GetType();
            var requiredProperties = RequiredPropertiesForType[type];
            var propertyNames = GetPropertiesByAttributeType(entity, requiredProperties);
            return propertyNames;
        }

        public static ICollection<string> GetExpectedProperties<T>(T entity)
        {
            var type = entity.GetType();
            var expectedProperties = ExpectedPropertiesForType[type];
            var propertyNames = GetPropertiesByAttributeType(entity, expectedProperties);
            return propertyNames;
        }

        private static List<string> GetPropertiesByAttributeType<T>(T entity, IEnumerable<PropertyInfo> propertiesByAttributeType)
        {
            var propertyNames = new List<string>();
            foreach (var property in propertiesByAttributeType)
            {
                var propertyType = property.PropertyType;
                var value = property.GetValue(entity);
                var defaultValue = DefaultValueCache.GetDefaultValue(propertyType);
                var uninitialised = value == null || value == defaultValue;
                if (uninitialised)
                    propertyNames.Add(property.Name);
            }
            return propertyNames;
        }

        private static ICollection<PropertyInfo> GetClassPropertiesByAttributeType<T>(Type type)
        {
            var propertiesWithAttribute = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                foreach (var attribute in property.CustomAttributes)
                {
                    if (attribute.AttributeType != typeof(T))
                        continue;
                    propertiesWithAttribute.Add(property);
                    break;
                }
            }
            return propertiesWithAttribute;
        }
    }
}
