namespace CardMaster.Framework.Buff
{
    /// <summary>
    /// Buff 定义基类，描述静态配置。
    /// </summary>
    public abstract class BaseBuff
    {
        public string BuffId { get; protected set; }
        public BuffMergePolicy MergePolicy { get; protected set; } = BuffMergePolicy.Replace;

        public abstract BuffInstance CreateInstance(object owner);
    }
}
