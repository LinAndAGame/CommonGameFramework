namespace CommonGameFramework.DragPlace
{
    /// <summary>失败回源的缓动。不依赖 DOTween。</summary>
    public enum DragEase
    {
        Linear = 0,
        OutQuad = 1
    }

    /// <summary>把 0~1 的进度映射成缓动值。</summary>
    public static class DragEasing
    {
        // 计算缓动后的进度。t 会被夹到 0~1。
        public static float Evaluate(DragEase ease, float t)
        {
            if (t <= 0f)
            {
                return 0f;
            }

            if (t >= 1f)
            {
                return 1f;
            }

            if (ease == DragEase.OutQuad)
            {
                float inv = 1f - t;
                return 1f - (inv * inv);
            }

            return t;
        }
    }
}
