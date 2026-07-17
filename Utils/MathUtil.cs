using UnityEngine;

namespace CommonGameFramework.Utils
{
    /// <summary>常用数值工具（与业务无关）。</summary>
    public static class MathUtil
    {
        /// <summary>将 value 从 [fromMin, fromMax] 线性映射到 [toMin, toMax]。</summary>
        public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            if (Mathf.Approximately(fromMax, fromMin))
                return toMin;

            var t = (value - fromMin) / (fromMax - fromMin);
            return toMin + t * (toMax - toMin);
        }

        /// <summary>映射并 Clamp 到目标区间。</summary>
        public static float RemapClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            var result = Remap(value, fromMin, fromMax, toMin, toMax);
            return Mathf.Clamp(result, Mathf.Min(toMin, toMax), Mathf.Max(toMin, toMax));
        }

        public static bool Approximately(float a, float b, float epsilon = 0.0001f)
        {
            return Mathf.Abs(a - b) <= epsilon;
        }

        /// <summary>环绕取模，结果落在 [0, length)。length &lt;= 0 时返回 0。</summary>
        public static int WrapIndex(int index, int length)
        {
            if (length <= 0) return 0;
            var m = index % length;
            return m < 0 ? m + length : m;
        }
    }
}
