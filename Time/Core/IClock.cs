namespace CommonGameFramework.Time
{
    /// <summary>
    /// 游戏时钟 seam；Pause / TimeScale 由本接口控制，不直接改 Unity.Time.timeScale（除非游戏层自行同步）。
    /// </summary>
    public interface IClock
    {
        /// <summary>缩放后的帧间隔；暂停时为 0。须每帧先 Tick。</summary>
        float DeltaTime { get; }

        /// <summary>不受 Pause / TimeScale 影响的帧间隔。</summary>
        float UnscaledDeltaTime { get; }

        /// <summary>累计缩放时间（暂停时不推进）。</summary>
        float Time { get; }

        /// <summary>累计真实时间（暂停仍推进）。</summary>
        float UnscaledTime { get; }

        float TimeScale { get; set; }

        bool IsPaused { get; set; }

        /// <summary>每帧调用一次，推进时间。通常由 ClockDriver 或游戏 Bootstrap 调用。</summary>
        void Tick();
    }
}
