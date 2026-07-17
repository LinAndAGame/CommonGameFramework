namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 整数属性修正器。PercentAdd 为整数百分点（20 = +20%），PercentMult 为百分乘数（200 = 2 倍）。
    /// </summary>
    public sealed class IntStatModifier : IStatModifier<int>
    {
        public string SourceId { get; }
        public ModifierType Type { get; }
        public int Value { get; }

        public IntStatModifier(string sourceId, ModifierType type, int value)
        {
            SourceId = sourceId;
            Type = type;
            Value = value;
        }
    }
}
