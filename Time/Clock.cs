using UnityEngine;

namespace CommonGameFramework.Time
{
    /// <summary>
    /// 全局时钟门面：调用方只碰本类；实现经 IClock（默认 UnityClock）。
    /// 首次访问时自动创建 [Clock] 并挂载 ClockDriver，无需场景预挂。
    /// </summary>
    public static class Clock
    {
        static IClock _current = new UnityClock();
        static bool _driverEnsured;

        public static IClock Current
        {
            get
            {
                EnsureDriver();
                return _current;
            }
            set
            {
                _current = value ?? new UnityClock();
                EnsureDriver();
            }
        }

        public static float DeltaTime
        {
            get
            {
                EnsureDriver();
                return _current.DeltaTime;
            }
        }

        public static float UnscaledDeltaTime
        {
            get
            {
                EnsureDriver();
                return _current.UnscaledDeltaTime;
            }
        }

        public static float Time
        {
            get
            {
                EnsureDriver();
                return _current.Time;
            }
        }

        public static float UnscaledTime
        {
            get
            {
                EnsureDriver();
                return _current.UnscaledTime;
            }
        }

        public static float TimeScale
        {
            get
            {
                EnsureDriver();
                return _current.TimeScale;
            }
            set
            {
                EnsureDriver();
                _current.TimeScale = value;
            }
        }

        public static bool IsPaused
        {
            get
            {
                EnsureDriver();
                return _current.IsPaused;
            }
            set
            {
                EnsureDriver();
                _current.IsPaused = value;
            }
        }

        public static void Tick() => _current.Tick();

        /// <summary>确保存在 [Clock] + ClockDriver（可重复调用）。</summary>
        public static void EnsureDriver()
        {
            if (_driverEnsured) return;
            if (Application.isPlaying == false) return;

            var existing = Object.FindObjectOfType<ClockDriver>();
            if (existing == null)
            {
                var go = new GameObject("[Clock]");
                Object.DontDestroyOnLoad(go);
                go.AddComponent<ClockDriver>();
            }

            _driverEnsured = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _driverEnsured = false;
            _current = new UnityClock();
        }
    }
}
