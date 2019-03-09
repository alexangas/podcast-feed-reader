using System;
using System.Collections.Generic;
using System.Reflection;

namespace PodcastFeedReader.Services
{
    public class MetadataCacheService : IMetadataService
    {
        private readonly object _initialiseCacheServiceLock;

        private readonly MethodInfo _getDefaultGenericMethodInfo;

        private readonly Dictionary<Type, Dictionary<Type, ICollection<PropertyInfo>>> _propertiesByAttributeForEntity;

        private readonly Dictionary<Type, object> _defaultValues;

        public MetadataCacheService()
        {
            _initialiseCacheServiceLock = new object();
            
            // http://stackoverflow.com/a/8022677/6651
            _getDefaultGenericMethodInfo = typeof(MetadataCacheService).GetMethod(nameof(GetDefaultGeneric), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            _propertiesByAttributeForEntity = new Dictionary<Type, Dictionary<Type, ICollection<PropertyInfo>>>(2);
            
            _defaultValues = new Dictionary<Type, object>();
        }

        public void Initialise(ICollection<Type> types, ICollection<Attribute> attributes)
        {
            lock (_initialiseCacheServiceLock)
            {
                foreach (var type in types)
                {
                    var propertiesByAttribute = new Dictionary<Type, ICollection<PropertyInfo>>();

                    foreach (var attribute in attributes)
                    {
                        var attributeType = attribute.GetType();
                        var properties = GetClassPropertiesByAttributeType(type, attributeType);
                        propertiesByAttribute[attributeType] = properties;
                        
                        foreach (var property in properties)
                        {
                            var propertyType = property.PropertyType;
                            if (!_defaultValues.ContainsKey(propertyType))
                            {
                                var defaultPropertyTypeValue = GenerateDefaultValue(propertyType);
                                _defaultValues[propertyType] = defaultPropertyTypeValue;
                            }
                        }
                    }

                    _propertiesByAttributeForEntity[type] = propertiesByAttribute;
                }
            }
        }

        public ICollection<string> GetPropertyNamesMissingValueByAttribute<TEntity, TAttribute>(TEntity entity)
            where TEntity : class
            where TAttribute : Attribute
        {
            var entityType = entity.GetType();
            var propertiesByAttribute = _propertiesByAttributeForEntity[entityType];
            var attributeType = typeof(TAttribute);
            var propertiesWithAttribute = propertiesByAttribute[attributeType];
            var propertyNames = GetPropertyNamesMissingValueByPropertyList(entity, propertiesWithAttribute);
            return propertyNames;
        }
        
        private List<string> GetPropertyNamesMissingValueByPropertyList<TEntity>(TEntity entity, IEnumerable<PropertyInfo> propertiesWithAttribute)
        {
            var propertyNames = new List<string>();
            foreach (var property in propertiesWithAttribute)
            {
                var propertyType = property.PropertyType;
                var value = property.GetValue(entity);
                var defaultValue = GetDefaultValue(propertyType);
                var uninitialised = value == null || value == defaultValue;
                if (uninitialised)
                    propertyNames.Add(property.Name);
            }
            return propertyNames;
        }

        private static ICollection<PropertyInfo> GetClassPropertiesByAttributeType(Type type, Type attributeType)
        {
            var propertiesWithAttribute = new List<PropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                foreach (var attribute in property.CustomAttributes)
                {
                    if (attribute.AttributeType != attributeType)
                        continue;
                    propertiesWithAttribute.Add(property);
                    break;
                }
            }
            return propertiesWithAttribute;
        }

        public object GetDefaultValue(Type type)
        {
            var defaultValue = _defaultValues[type];
            return defaultValue;
        }

        private object GenerateDefaultValue(Type type)
        {
            var baseType = type;
            var nullableUnderlying = Nullable.GetUnderlyingType(type);
            if (nullableUnderlying != null)
                baseType = nullableUnderlying;

            var makeGenericMethod = _getDefaultGenericMethodInfo.MakeGenericMethod(baseType);

            var defaultValue = makeGenericMethod.Invoke(typeof(MetadataCacheService), null);
            return defaultValue;
        }

        private static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}
