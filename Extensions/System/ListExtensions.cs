using System;
using System.Collections.Generic;

namespace CommonGameFramework.Extensions
{
    /// <summary>IList 扩展。</summary>
    public static class ListExtensions
    {
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            if (indexA < 0 || indexA >= list.Count) throw new ArgumentOutOfRangeException(nameof(indexA));
            if (indexB < 0 || indexB >= list.Count) throw new ArgumentOutOfRangeException(nameof(indexB));

            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }

        /// <summary>Fisher–Yates 洗牌；random 为 null 时用 Rng.Current。</summary>
        public static void Shuffle<T>(this IList<T> list, Random.IRandom random = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));
            random ??= Random.Rng.Current;

            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static T GetRandomOrDefault<T>(this IList<T> list, Random.IRandom random = null)
        {
            if (list == null || list.Count == 0) return default;
            random ??= Random.Rng.Current;
            return list[random.Range(0, list.Count)];
        }
    }
}
