using UnityEngine;

namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// PlayerPrefs 本地存档；换机不跟 Steam 云。
    /// </summary>
    public sealed class PlayerPrefsAchievementLocal : IAchievementLocal
    {
        const string UnlockedSuffix = "_unlocked";
        const string ProgressSuffix = "_progress";

        readonly string _keyPrefix;

        public PlayerPrefsAchievementLocal(string keyPrefix = "ach_")
        {
            _keyPrefix = string.IsNullOrEmpty(keyPrefix) ? "ach_" : keyPrefix;
        }

        public int GetProgress(AchievementId id)
        {
            if (id == null) return 0;
            return PlayerPrefs.GetInt(ProgressKey(id), 0);
        }

        public void SetProgress(AchievementId id, int value)
        {
            if (id == null) return;
            PlayerPrefs.SetInt(ProgressKey(id), value);
        }

        public bool IsUnlocked(AchievementId id)
        {
            return id != null && PlayerPrefs.GetInt(UnlockedKey(id), 0) == 1;
        }

        public void MarkUnlocked(AchievementId id)
        {
            if (id == null) return;
            PlayerPrefs.SetInt(UnlockedKey(id), 1);
        }

        public void Load() { }

        public void Save() => PlayerPrefs.Save();

        string ProgressKey(AchievementId id) => _keyPrefix + id.Key + ProgressSuffix;
        string UnlockedKey(AchievementId id) => _keyPrefix + id.Key + UnlockedSuffix;
    }
}
