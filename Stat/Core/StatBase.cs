namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 属性基类，持有唯一 Aggregator 实例，读写与修正均委托给聚合器。
    /// </summary>
    public class StatBase<TValue, TModifier> where TModifier : IStatModifier<TValue>
    {
        readonly IStatAggregator<TValue, TModifier> _aggregator;

        public IStatAggregator<TValue, TModifier> Aggregator => _aggregator;

        protected StatBase(IStatAggregator<TValue, TModifier> aggregator)
        {
            _aggregator = aggregator ?? throw new System.ArgumentNullException(nameof(aggregator));
        }

        public TValue BaseValue
        {
            get => _aggregator.BaseValue;
            set => _aggregator.BaseValue = value;
        }

        public TValue Value => _aggregator.Value;

        public void AddModifier(TModifier modifier)
        {
            _aggregator.AddModifier(modifier);
        }

        public void RemoveModifiersFromSource(string sourceId)
        {
            _aggregator.RemoveModifiersFromSource(sourceId);
        }

        public void ClearModifiers()
        {
            _aggregator.ClearModifiers();
        }
    }
}
