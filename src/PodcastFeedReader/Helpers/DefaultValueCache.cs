using System;
using System.Collections.Generic;
using System.Reflection;

namespace PodcastFeedReader.Helpers
{
    public static class DefaultValueCache
    {
        private static readonly MethodInfo GetDefaultGenericMethodInfo;

        private static readonly object DefaultValuesLock;

        private static readonly Dictionary<Type, object> DefaultValues;

        static DefaultValueCache()
        {
            // http://stackoverflow.com/a/8022677/6651
            GetDefaultGenericMethodInfo = typeof(DefaultValueCache).GetMethod(nameof(GetDefaultGeneric), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            DefaultValuesLock = new object();
            DefaultValues = new Dictionary<Type, object>();
        }

        public static void AddDefaultValuesToCache(IEnumerable<Type> types)
        {
            lock (DefaultValuesLock)
            {
                foreach (var type in types)
                {
                    var value = GenerateDefaultValue(type);
                    DefaultValues[type] = value;
                }
            }
        }

        public static object GetDefaultValue(Type type)
        {
            var defaultValue = DefaultValues[type];
            return defaultValue;
        }

        private static object GenerateDefaultValue(Type type)
        {
            var baseType = type;
            var nullableUnderlying = Nullable.GetUnderlyingType(type);
            if (nullableUnderlying != null)
                baseType = nullableUnderlying;

            var makeGenericMethod = GetDefaultGenericMethodInfo.MakeGenericMethod(baseType);

            var defaultValue = makeGenericMethod.Invoke(typeof(DefaultValueCache), null);
            return defaultValue;
        }

        private static T GetDefaultGeneric<T>()
        {
            return default(T);
        }
    }
}
