using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 对象池公共契约：快照与预加载。
    /// </summary>
    internal interface IPool
    {
        PoolSnapshot GetSnapshot();
        void Preload(int count);
    }

    internal interface IClassPool : IPool
    {
        void Release(IPoolable item);
    }

    internal interface IComponentPool : IPool
    {
        void Release(Component component);
    }
}
