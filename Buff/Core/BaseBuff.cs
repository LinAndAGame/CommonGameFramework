namespace CommonGameFramework.Buff
{
    /// <summary>
    /// Buff 定义基类，描述静态配置。
    /// </summary>
    public abstract class BaseBuff
    {
        public string BuffId { get; protected set; }
        public BuffMergePolicy MergePolicy { get; protected set; } = ReplaceBuffMergePolicy.Instance;

        public abstract BuffInstance CreateInstance(object owner);
    }
}
