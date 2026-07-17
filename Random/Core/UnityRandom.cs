using System;
using UnityEngine;

namespace CommonGameFramework.Random
{
    /// <summary>
    /// 基于 UnityEngine.Random 的实现；与引擎随机状态共用，适合运行时玩法。
    /// </summary>
    public sealed class UnityRandom : IRandom
    {
        public int Seed { get; private set; }

        public UnityRandom(int seed)
        {
            Reseed(seed);
        }

        public UnityRandom()
            : this(Environment.TickCount)
        {
        }

        /// <summary>UnityEngine.Random.value，范围为 [0, 1]。</summary>
        public float Value => UnityEngine.Random.value;

        public int Range(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentException("minInclusive 须小于 maxExclusive。");

            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            if (minInclusive > maxInclusive)
                throw new ArgumentException("minInclusive 须小于等于 maxInclusive。");

            return UnityEngine.Random.Range(minInclusive, maxInclusive);
        }

        public void Reseed(int seed)
        {
            Seed = seed;
            UnityEngine.Random.InitState(seed);
        }
    }
}
