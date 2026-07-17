using UnityEngine;

namespace CommonGameFramework.Pool
{
    public enum PoolKind
    {
        CSharp,
        Mono,
        Component
    }

    /// <summary>
    /// 对象池运行时快照，供 Editor 调试窗口展示。
    /// </summary>
    public readonly struct PoolSnapshot
    {
        public string Name { get; }
        public PoolKind Kind { get; }
        public int InactiveCount { get; }
        public int ActiveCount { get; }
        public int TotalCreated { get; }
        public GameObject Prefab { get; }

        public PoolSnapshot(string name, PoolKind kind, int inactiveCount, int activeCount, int totalCreated, GameObject prefab)
        {
            Name = name;
            Kind = kind;
            InactiveCount = inactiveCount;
            ActiveCount = activeCount;
            TotalCreated = totalCreated;
            Prefab = prefab;
        }
    }
}
