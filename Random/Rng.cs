using System;

namespace CommonGameFramework.Random
{
    /// <summary>
    /// 全局随机门面：调用方只碰本类；实现经 IRandom（默认 UnityRandom）。
    /// 命名 Rng，避免与 System.Random / UnityEngine.Random 混淆。
    /// 单测请替换为 CSharpRandom(固定种子)。
    /// </summary>
    public static class Rng
    {
        static IRandom _current = new UnityRandom();

        public static IRandom Current
        {
            get => _current;
            set => _current = value ?? new UnityRandom();
        }

        public static int Seed => _current.Seed;

        public static float Value => _current.Value;

        public static int Range(int minInclusive, int maxExclusive) =>
            _current.Range(minInclusive, maxExclusive);

        public static float Range(float minInclusive, float maxInclusive) =>
            _current.Range(minInclusive, maxInclusive);

        public static void Reseed(int seed) => _current.Reseed(seed);

        /// <summary>用当前时间种子重新播种（非确定性）。</summary>
        public static void ReseedFromTime() => _current.Reseed(Environment.TickCount);
    }
}
