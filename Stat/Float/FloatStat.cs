namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 浮点属性，默认使用 <see cref="FloatStatAggregator"/>，可注入自定义聚合器。
    /// </summary>
    public class FloatStat : StatBase<float, IStatModifier<float>>
    {
        public FloatStat()
            : base(new FloatStatAggregator())
        {
        }

        public FloatStat(IStatAggregator<float, IStatModifier<float>> aggregator)
            : base(aggregator)
        {
        }
    }
}
