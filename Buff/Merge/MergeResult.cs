namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 同 ID Buff 合并结果；由 BuffContainer 编排后续 Remove / Add。
    /// </summary>
    public enum MergeResult
    {
        /// <summary>已合并到 existing，丢弃 incoming。</summary>
        Merged,

        /// <summary>需移除 existing，再添加 incoming。</summary>
        Replaced,

        /// <summary>未合并，incoming 作为独立实例添加；后续 Find/合并仍只匹配第一个同 BuffId。</summary>
        NotMerged,
    }
}
