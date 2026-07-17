namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 本地存档 seam（进度 / 解锁）。仅给 AchievementService 与自定义存档 adapter 用；
    /// 游戏逻辑请走 AchievementService，勿直接改本地以免绕过解锁规则。
    /// </summary>
    public interface IAchievementLocal
    {
        int GetProgress(AchievementId id);
        void SetProgress(AchievementId id, int value);
        bool IsUnlocked(AchievementId id);
        void MarkUnlocked(AchievementId id);
        void Load();
        void Save();
    }
}
