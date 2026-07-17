using UnityEngine;

namespace CommonGameFramework.Time
{
    /// <summary>
    /// 默认时钟：基于 Unity unscaledDeltaTime，自管 Pause 与 TimeScale。
    /// </summary>
    public sealed class UnityClock : IClock
    {
        public float DeltaTime { get; private set; }
        public float UnscaledDeltaTime { get; private set; }
        public float Time { get; private set; }
        public float UnscaledTime { get; private set; }

        float _timeScale = 1f;

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = value < 0f ? 0f : value;
        }

        public bool IsPaused { get; set; }

        public void Tick()
        {
            UnscaledDeltaTime = UnityEngine.Time.unscaledDeltaTime;
            UnscaledTime += UnscaledDeltaTime;

            DeltaTime = IsPaused ? 0f : UnscaledDeltaTime * TimeScale;
            Time += DeltaTime;
        }

        /// <summary>测试或切场景时重置累计时间。</summary>
        public void ResetTime()
        {
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            Time = 0f;
            UnscaledTime = 0f;
        }
    }
}
