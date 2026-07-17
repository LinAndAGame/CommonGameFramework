namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 外部平台成就同步 seam（Steam / WeGame 等）。
    /// SDK 配置在各 adapter 构造时注入；离线则不挂载（AttachPlatform(null)）。
    /// </summary>
    public interface IAchievementPlatform
    {
        /// <summary>平台 SDK 是否可用。</summary>
        bool IsAvailable { get; }

        void Initialize();

        void Unlock(AchievementId id);

        void ReportProgress(AchievementId id, int current, int target);

        void Flush();
    }
}
