using System;
using System.Collections.Generic;

namespace CommonGameFramework.Extensions
{
    /// <summary>Dictionary 扩展。</summary>
    public static class DictionaryExtensions
    {
        /// <summary>键不存在时用 factory 创建并写入，再返回。</summary>
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TKey, TValue> factory)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            if (dictionary.TryGetValue(key, out var value) == false)
            {
                value = factory(key);
                dictionary[key] = value;
            }

            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue = default)
        {
            if (dictionary == null) return defaultValue;
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}
