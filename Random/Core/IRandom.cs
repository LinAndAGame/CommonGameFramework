namespace CommonGameFramework.Random
{
    /// <summary>
    /// 可注入随机数 seam；测试可固定种子。
    /// </summary>
    public interface IRandom
    {
        int Seed { get; }

        /// <summary>[0, 1) 浮点。</summary>
        float Value { get; }

        /// <summary>[minInclusive, maxExclusive) 整数。</summary>
        int Range(int minInclusive, int maxExclusive);

        /// <summary>[minInclusive, maxInclusive] 浮点。</summary>
        float Range(float minInclusive, float maxInclusive);

        /// <summary>重新设定种子并重置序列。</summary>
        void Reseed(int seed);
    }
}
