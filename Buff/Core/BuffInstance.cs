using System;
using UnityEngine;

namespace CommonGameFramework.Buff
{
    public abstract class BuffInstance
    {
        public BaseBuff Definition { get; }
        public object Owner { get; }
        public string BuffId => Definition?.BuffId;
        public int CurLayer { get; private set; } = 1;
        public IExpirable Duration { get; private set; }

        /// <summary>Buff 是否处于激活状态；未激活时不应产生效果，但可保留在容器中。</summary>
        public bool Active { get; private set; }

        /// <summary>层数变化（合并叠层等）；Presentation 可订阅。</summary>
        public event Action<int> LayerChanged;

        protected BuffInstance(BaseBuff definition, object owner)
        {
            Definition = definition;
            Owner = owner;
        }

        /// <summary>施加到容器时调用（初始化）。</summary>
        public virtual void Init() { }

        /// <summary>从容器移除时调用（清理）。</summary>
        public virtual void OnRemove() { }

        /// <summary>合并到 existing 后 incoming 被丢弃时调用（释放未入队资源）。</summary>
        public virtual void OnMergeDiscarded() { }

        /// <summary>切换激活状态；状态变化时调用 OnActiveHandle / OnNotActiveHandle。</summary>
        public void SetActive(bool active)
        {
            if (Active == active) return;
            Active = active;
            if (active)
                OnActiveHandle();
            else
                OnNotActiveHandle();
        }

        /// <summary>激活时回调；子类挂接 Stat、开启监听等。</summary>
        protected virtual void OnActiveHandle() { }

        /// <summary>关闭时回调；子类卸接 Stat、关闭监听等。</summary>
        protected virtual void OnNotActiveHandle() { }

        /// <summary>尝试触发；CanTrigger 为 false 时不执行 OnTriggerHandle。</summary>
        public bool TryTrigger()
        {
            if (!CanTrigger()) return false;
            OnTriggerHandle();
            return true;
        }

        /// <summary>是否允许触发；子类按层数、CD、上下文等自行判定。</summary>
        public virtual bool CanTrigger() => Active;

        /// <summary>触发成功时回调；子类实现具体效果。</summary>
        protected virtual void OnTriggerHandle() { }

        /// <summary>未激活时默认不推进 Duration；子类可 override（如暂停效果但计时继续）。</summary>
        protected virtual bool ShouldAdvanceDuration => Active;

        public void TickRealtime(float deltaTime)
        {
            if (!ShouldAdvanceDuration || Duration == null) return;
            if (Duration is IRealtimeDuration realtime)
                realtime.Tick(deltaTime);
            else if (Duration is ITurnBasedDuration)
                WarnDurationMismatch(nameof(TickRealtime), nameof(IRealtimeDuration));
        }

        public void AdvanceTurn()
        {
            if (!ShouldAdvanceDuration || Duration == null) return;
            if (Duration is ITurnBasedDuration turnBased)
                turnBased.AdvanceTurn();
            else if (Duration is IRealtimeDuration)
                WarnDurationMismatch(nameof(AdvanceTurn), nameof(ITurnBasedDuration));
        }

        /// <summary>Duration == null 时永不过期。</summary>
        public bool IsExpired => Duration != null && Duration.IsExpired;

        /// <summary>设置 Duration（施加前由游戏层或工厂调用）。</summary>
        public void SetDuration(IExpirable duration) => Duration = duration;

        internal void MergeRefreshDuration(IExpirable duration) => Duration = duration;

        internal void MergeStackLayer()
        {
            CurLayer++;
            LayerChanged?.Invoke(CurLayer);
        }

        void WarnDurationMismatch(string caller, string expected)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[Buff] {BuffId}: {caller} 需要 {expected}，当前 Duration 类型不匹配。");
#endif
        }
    }
}
