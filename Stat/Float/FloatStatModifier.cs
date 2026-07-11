namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 浮点属性修正器。PercentAdd 为小数比例（0.2 = +20%），PercentMult 为乘数（2 = 2 倍）。
    /// </summary>
    public class FloatStatModifier : IStatModifier<float>
    {
        public string SourceId { get; }
        public ModifierType Type { get; }
        public float Value { get; }

        public FloatStatModifier(string sourceId, ModifierType type, float value)
        {
            SourceId = sourceId;
            Type = type;
            Value = value;
        }
    }
}
