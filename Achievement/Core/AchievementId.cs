namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 成就标识基类；游戏项目派生子类，Key 用于本地存档与平台映射。
    /// </summary>
    public abstract class AchievementId
    {
        public abstract string Key { get; }

        public override bool Equals(object obj)
        {
            return obj is AchievementId other && Key == other.Key;
        }

        public override int GetHashCode() => Key?.GetHashCode() ?? 0;

        public override string ToString() => Key;
    }
}
