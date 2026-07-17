using System;
using System.Collections.Generic;

namespace CommonGameFramework.Buff
{
    /// <summary>
    /// Buff 容器；实现 <see cref="IBuffContainer"/>。
    /// </summary>
    /// <remarks>
    /// TryAdd 返回 true 表示「本请求已处理」（含 Merged 丢弃 incoming），不等于「一定触发了 BuffAdded」。
    /// <list type="bullet">
    /// <item>按 BuffId 查找<strong>第一个</strong> existing；合并策略取自 incoming 的 Definition.MergePolicy。</item>
    /// <item><see cref="MergeResult.Merged"/>：合并到 existing，incoming.OnMergeDiscarded()，不 Add、不触发 BuffAdded。</item>
    /// <item><see cref="MergeResult.Replaced"/>：Remove(existing)，再 Init → SetActive → Add incoming。</item>
    /// <item><see cref="MergeResult.NotMerged"/>：与 existing 共存；后续合并仍只认列表中第一个同 BuffId。</item>
    /// </list>
    /// </remarks>
    public class BuffContainer : IBuffContainer
    {
        readonly List<BuffInstance> _buffs = new List<BuffInstance>();

        public IReadOnlyList<BuffInstance> Buffs => _buffs;

        public event Action<BuffInstance> BuffAdded;
        public event Action<BuffInstance> BuffRemoved;
        public event Action<BuffInstance> BuffExpired;

        public bool TryAdd(BuffInstance instance)
        {
            if (instance == null) return false;
            var existing = _buffs.Find(b => b.Definition.BuffId == instance.Definition.BuffId);
            if (existing != null)
            {
                switch (instance.Definition.MergePolicy.TryMerge(existing, instance))
                {
                    case MergeResult.Merged:
                        instance.OnMergeDiscarded();
                        return true;
                    case MergeResult.Replaced:
                        Remove(existing);
                        break;
                }
            }

            instance.Init();
            instance.SetActive(true);
            _buffs.Add(instance);
            BuffAdded?.Invoke(instance);
            return true;
        }

        public void Remove(BuffInstance instance)
        {
            if (instance == null || !_buffs.Contains(instance)) return;
            if (instance.Active)
                instance.SetActive(false);
            instance.OnRemove();
            _buffs.Remove(instance);
            BuffRemoved?.Invoke(instance);
        }

        public void TickRealtime(float deltaTime)
        {
            for (var i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.TickRealtime(deltaTime);
                if (buff.IsExpired)
                    RemoveExpired(buff);
            }
        }

        public void AdvanceTurn()
        {
            for (var i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.AdvanceTurn();
                if (buff.IsExpired)
                    RemoveExpired(buff);
            }
        }

        void RemoveExpired(BuffInstance instance)
        {
            BuffExpired?.Invoke(instance);
            Remove(instance);
        }
    }
}
