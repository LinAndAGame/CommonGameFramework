namespace CommonGameFramework.Stat
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
}
