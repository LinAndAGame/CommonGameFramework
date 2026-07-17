namespace CommonGameFramework.Cooldown
{
    /// <summary>
    /// 冷却标识基类；游戏项目派生子类，Key 用于 Tracker 字典。
    /// </summary>
    public abstract class CooldownId
    {
        public abstract string Key { get; }

        public override bool Equals(object obj)
        {
            return obj is CooldownId other && Key == other.Key;
        }

        public override int GetHashCode() => Key?.GetHashCode() ?? 0;

        public override string ToString() => Key;
    }
}
