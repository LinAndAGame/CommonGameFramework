using System;
using System.Collections.Generic;

namespace CommonGameFramework.Buff
{
    /// <summary>
    /// Buff 容器 seam；经 services bag 注入，测试可替换 in-memory adapter。
    /// </summary>
    public interface IBuffContainer
    {
        IReadOnlyList<BuffInstance> Buffs { get; }

        event Action<BuffInstance> BuffAdded;
        event Action<BuffInstance> BuffRemoved;
        event Action<BuffInstance> BuffExpired;

        /// <summary>
        /// 尝试添加 Buff。true = 已处理（含 Merged）；false = instance 为 null。
        /// Merged 时不触发 BuffAdded；NotMerged 时同 ID 可多实例，但合并仍只匹配第一个。
        /// </summary>
        bool TryAdd(BuffInstance instance);
        void Remove(BuffInstance instance);
        void TickRealtime(float deltaTime);
        void AdvanceTurn();
    }
}
