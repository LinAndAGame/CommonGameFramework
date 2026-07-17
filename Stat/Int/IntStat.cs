namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 整数属性，默认使用 <see cref="IntStatAggregator"/>，可注入自定义聚合器。
    /// </summary>
    public class IntStat : StatBase<int, IStatModifier<int>>
    {
        public IntStat()
            : base(new IntStatAggregator())
        {
        }

        public IntStat(IStatAggregator<int, IStatModifier<int>> aggregator)
            : base(aggregator)
        {
        }
    }
}
