namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 同 ID Buff 合并策略；游戏项目可派生子类实现自定义合并逻辑。
    /// </summary>
    public abstract class BuffMergePolicy
    {
        /// <summary>
        /// 尝试将 incoming 合并到 existing；通过 BuffInstance 内部 mutator 修改状态，不操作容器。
        /// </summary>
        public abstract MergeResult TryMerge(BuffInstance existing, BuffInstance incoming);
    }

    /// <summary>移除旧 Buff，再添加新 Buff。</summary>
    public sealed class ReplaceBuffMergePolicy : BuffMergePolicy
    {
        public static ReplaceBuffMergePolicy Instance { get; } = new ReplaceBuffMergePolicy();

        public override MergeResult TryMerge(BuffInstance existing, BuffInstance incoming) => MergeResult.Replaced;
    }

    /// <summary>保留旧 Buff，刷新 Duration（浅拷贝 incoming.Duration 引用）。</summary>
    public sealed class RefreshDurationBuffMergePolicy : BuffMergePolicy
    {
        public static RefreshDurationBuffMergePolicy Instance { get; } = new RefreshDurationBuffMergePolicy();

        public override MergeResult TryMerge(BuffInstance existing, BuffInstance incoming)
        {
            existing.MergeRefreshDuration(incoming.Duration);
            return MergeResult.Merged;
        }
    }

    /// <summary>保留旧 Buff，层数 +1（不刷新 Duration）。</summary>
    public sealed class StackBuffMergePolicy : BuffMergePolicy
    {
        public static StackBuffMergePolicy Instance { get; } = new StackBuffMergePolicy();

        public override MergeResult TryMerge(BuffInstance existing, BuffInstance incoming)
        {
            existing.MergeStackLayer();
            return MergeResult.Merged;
        }
    }
}
