using System.Collections.Generic;

namespace CommonGameFramework.Extensions
{
    /// <summary>ICollection 扩展。</summary>
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
