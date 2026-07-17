using System;
using System.Collections.Generic;
using CommonGameFramework.Random;

namespace CommonGameFramework.Utils
{
    /// <summary>加权随机等通用算法。</summary>
    public static class WeightedRandom
    {
        /// <summary>
        /// 按权重抽取下标；weights 与选项一一对应，权重 &lt;= 0 视为 0。
        /// 全部权重为 0 或空时返回 -1。
        /// </summary>
        public static int PickIndex(IReadOnlyList<float> weights, IRandom random = null)
        {
            if (weights == null || weights.Count == 0) return -1;
            random ??= Rng.Current;

            var total = 0f;
            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] > 0f)
                    total += weights[i];
            }

            if (total <= 0f) return -1;

            var roll = random.Range(0f, total);
            var cursor = 0f;
            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] <= 0f) continue;
                cursor += weights[i];
                if (roll < cursor)
                    return i;
            }

            return weights.Count - 1;
        }

        /// <summary>按权重抽取元素；失败返回 default。</summary>
        public static T Pick<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights, IRandom random = null)
        {
            if (items == null || weights == null || items.Count != weights.Count)
                throw new ArgumentException("items 与 weights 长度须一致且非 null。");

            var index = PickIndex(weights, random);
            return index < 0 ? default : items[index];
        }
    }
}
