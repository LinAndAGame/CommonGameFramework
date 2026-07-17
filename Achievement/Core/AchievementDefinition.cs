namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 成就定义：TargetValue == 1 为一次性解锁；&gt; 1 为进度型。
    /// </summary>
    public sealed class AchievementDefinition
    {
        public AchievementId Id { get; }
        public int TargetValue { get; }

        /// <summary>是否为进度型成就（TargetValue &gt; 1）。</summary>
        public bool IsIncremental => TargetValue > 1;

        public AchievementDefinition(AchievementId id, int targetValue = 1)
        {
            Id = id;
            TargetValue = targetValue < 1 ? 1 : targetValue;
        }
    }
}
