using System.Collections.Generic;

namespace CommonGameFramework.Cooldown
{
    /// <summary>
    /// 多冷却管理器：按 CooldownId 启停与查询。经 services bag 注入或游戏层持有。
    /// </summary>
    public sealed class CooldownTracker
    {
        readonly Dictionary<string, CooldownTimer> _timers = new Dictionary<string, CooldownTimer>();

        public void Start(CooldownId id, float duration)
        {
            if (id == null) return;
            GetOrCreate(id.Key).Start(duration);
        }

        public void Tick(float deltaTime)
        {
            foreach (var timer in _timers.Values)
                timer.Tick(deltaTime);
        }

        public bool IsReady(CooldownId id)
        {
            if (id == null) return true;
            return _timers.TryGetValue(id.Key, out var timer) == false || timer.IsReady;
        }

        public bool IsActive(CooldownId id)
        {
            if (id == null) return false;
            return _timers.TryGetValue(id.Key, out var timer) && timer.IsActive;
        }

        public float GetRemaining(CooldownId id)
        {
            if (id == null) return 0f;
            return _timers.TryGetValue(id.Key, out var timer) ? timer.Remaining : 0f;
        }

        public float GetProgress(CooldownId id)
        {
            if (id == null) return 1f;
            return _timers.TryGetValue(id.Key, out var timer) ? timer.Progress : 1f;
        }

        public void Clear(CooldownId id)
        {
            if (id == null) return;
            if (_timers.TryGetValue(id.Key, out var timer))
                timer.Clear();
        }

        public void ClearAll()
        {
            foreach (var timer in _timers.Values)
                timer.Clear();
        }

        CooldownTimer GetOrCreate(string key)
        {
            if (_timers.TryGetValue(key, out var timer) == false)
            {
                timer = new CooldownTimer();
                _timers[key] = timer;
            }

            return timer;
        }
    }
}
