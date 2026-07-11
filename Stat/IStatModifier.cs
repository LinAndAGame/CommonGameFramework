namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 属性修正器接口，Value 类型与目标 Stat 一致。
    /// </summary>
    public interface IStatModifier<TValue>
    {
        string SourceId { get; }
        ModifierType Type { get; }
        TValue Value { get; }
    }

    /// <summary>
    /// 浮点修正器别名，保持现有 Buff / Command 调用不变。
    /// </summary>
    public interface IStatModifier : IStatModifier<float>
    {
    }
}
