namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 属性聚合器接口，Stat 持有唯一实例，可注入自定义实现。
    /// </summary>
    public interface IStatAggregator<TValue, TModifier> where TModifier : IStatModifier<TValue>
    {
        TValue BaseValue { get; set; }
        TValue Value { get; }
        void AddModifier(TModifier modifier);
        void RemoveModifiersFromSource(string sourceId);
        void ClearModifiers();
    }
}
