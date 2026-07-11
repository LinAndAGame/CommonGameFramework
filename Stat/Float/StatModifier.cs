namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 向后兼容别名，等价于 <see cref="FloatStatModifier"/>。
    /// </summary>
    public sealed class StatModifier : FloatStatModifier, IStatModifier
    {
        public StatModifier(string sourceId, ModifierType type, float value)
            : base(sourceId, type, value)
        {
        }
    }
}
