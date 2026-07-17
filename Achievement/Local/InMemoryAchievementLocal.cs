using System.Collections.Generic;

namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 内存版本地存档；测试 / 不落盘场景。
    /// </summary>
    public sealed class InMemoryAchievementLocal : IAchievementLocal
    {
        readonly Dictionary<string, int> _progress = new Dictionary<string, int>();
        readonly HashSet<string> _unlocked = new HashSet<string>();

        public int GetProgress(AchievementId id)
        {
            if (id == null) return 0;
            return _progress.TryGetValue(id.Key, out var v) ? v : 0;
        }

        public void SetProgress(AchievementId id, int value)
        {
            if (id == null) return;
            _progress[id.Key] = value;
        }

        public bool IsUnlocked(AchievementId id)
        {
            return id != null && _unlocked.Contains(id.Key);
        }

        public void MarkUnlocked(AchievementId id)
        {
            if (id == null) return;
            _unlocked.Add(id.Key);
        }

        public void Load() { }

        public void Save() { }
    }
}
