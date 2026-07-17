namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 布尔属性修正器：按 SourceId 覆盖，聚合时任一 true 则 true；Type 固定 Flat（Bool 不使用百分比语义）。
    /// </summary>
    public sealed class BoolStatModifier : IStatModifier<bool>
    {
        public string SourceId { get; }
        public ModifierType Type => ModifierType.Flat;
        public bool Value { get; }

        public BoolStatModifier(string sourceId, bool value)
        {
            SourceId = sourceId;
            Value = value;
        }
    }
}
