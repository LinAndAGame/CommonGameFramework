namespace CommonGameFramework.Cooldown
{
    /// <summary>
    /// 单个冷却计时器；由外部传入 deltaTime（通常用 Clock.DeltaTime），不依赖具体时钟实现。
    /// </summary>
    public sealed class CooldownTimer
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }

        public bool IsReady => Remaining <= 0f;
        public bool IsActive => Remaining > 0f;
        public float Progress => Duration <= 0f ? 1f : 1f - (Remaining / Duration);

        /// <summary>开始或刷新冷却。</summary>
        public void Start(float duration)
        {
            Duration = duration < 0f ? 0f : duration;
            Remaining = Duration;
        }

        /// <summary>推进冷却；deltaTime 通常取 Clock.DeltaTime。</summary>
        public void Tick(float deltaTime)
        {
            if (Remaining <= 0f || deltaTime <= 0f) return;
            Remaining -= deltaTime;
            if (Remaining < 0f) Remaining = 0f;
        }

        /// <summary>立刻就绪。</summary>
        public void Clear()
        {
            Remaining = 0f;
        }
    }
}
