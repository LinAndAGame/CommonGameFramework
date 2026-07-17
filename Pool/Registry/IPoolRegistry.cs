using System.Collections.Generic;
using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 对象池 registry seam；测试时可替换为 in-memory adapter。
    /// </summary>
    public interface IPoolRegistry
    {
        T Get<T>() where T : class, IPoolable, new();
        void Release(IPoolable item);
        void Preload<T>(int count = 1) where T : class, IPoolable, new();

        GameObject Get(GameObject prefab);
        T Get<T>(T prefab) where T : Component;
        T Get<T>(GameObject prefab) where T : Component;
        void Release(GameObject instance);

        void Preload(GameObject prefab, int count = 1);
        void Preload<T>(T prefab, int count = 1) where T : Component;
        void Preload<T>(GameObject prefab, int count = 1) where T : Component;

        IReadOnlyList<PoolSnapshot> GetSnapshots();
        void Reset();
    }
}
