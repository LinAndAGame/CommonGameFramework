using System;

namespace CommonGameFramework.Random
{
    /// <summary>
    /// 基于 System.Random 的可种子实现；适合单测与确定性逻辑。
    /// </summary>
    public sealed class CSharpRandom : IRandom
    {
        System.Random _random;

        public int Seed { get; private set; }

        public CSharpRandom(int seed)
        {
            Reseed(seed);
        }

        public CSharpRandom()
            : this(Environment.TickCount)
        {
        }

        public float Value => (float)_random.NextDouble();

        public int Range(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new ArgumentException("minInclusive 须小于 maxExclusive。");

            return _random.Next(minInclusive, maxExclusive);
        }

        public float Range(float minInclusive, float maxInclusive)
        {
            if (minInclusive > maxInclusive)
                throw new ArgumentException("minInclusive 须小于等于 maxInclusive。");

            if (minInclusive == maxInclusive)
                return minInclusive;

            return minInclusive + (float)_random.NextDouble() * (maxInclusive - minInclusive);
        }

        public void Reseed(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }
    }
}
