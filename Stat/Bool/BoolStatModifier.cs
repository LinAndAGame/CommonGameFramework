namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 布尔属性修正器，Value 表示该来源强制结果。
    /// </summary>
    public sealed class BoolStatModifier : IStatModifier<bool>
    {
        public string SourceId { get; }
        public ModifierType Type { get; }
        public bool Value { get; }

        public BoolStatModifier(string sourceId, bool value, ModifierType type = ModifierType.Flat)
        {
            SourceId = sourceId;
            Type = type;
            Value = value;
        }
    }
}
