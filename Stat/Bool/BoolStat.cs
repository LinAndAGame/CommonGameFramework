namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 布尔属性，默认使用 <see cref="BoolStatAggregator"/>，可注入自定义聚合器。
    /// </summary>
    public class BoolStat : StatBase<bool, IStatModifier<bool>>
    {
        public BoolStat()
            : base(new BoolStatAggregator())
        {
        }

        public BoolStat(IStatAggregator<bool, IStatModifier<bool>> aggregator)
            : base(aggregator)
        {
        }
    }
}
