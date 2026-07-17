using System.Collections.Generic;
using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 全局静态对象池门面：调用方只碰本类；实现经 IPoolRegistry → Internal 池。
    /// </summary>
    public static class Pool
    {
        static IPoolRegistry _registry = new DefaultPoolRegistry();

        /// <summary>当前 registry adapter；测试或多域时替换。</summary>
        public static IPoolRegistry Registry
        {
            get => _registry;
            set => _registry = value ?? new DefaultPoolRegistry();
        }

        public static T Get<T>() where T : class, IPoolable, new() => _registry.Get<T>();

        public static void Release(IPoolable item) => _registry.Release(item);

        public static void Preload<T>(int count = 1) where T : class, IPoolable, new() => _registry.Preload<T>(count);

        public static GameObject Get(GameObject prefab) => _registry.Get(prefab);

        public static T Get<T>(T prefab) where T : Component => _registry.Get(prefab);

        public static T Get<T>(GameObject prefab) where T : Component => _registry.Get<T>(prefab);

        public static void Release(GameObject instance) => _registry.Release(instance);

        public static void Preload(GameObject prefab, int count = 1) => _registry.Preload(prefab, count);

        public static void Preload<T>(T prefab, int count = 1) where T : Component => _registry.Preload(prefab, count);

        public static void Preload<T>(GameObject prefab, int count = 1) where T : Component => _registry.Preload<T>(prefab, count);

        public static IReadOnlyList<PoolSnapshot> GetSnapshots() => _registry.GetSnapshots();

        internal static void ResetStatics() => _registry.Reset();
    }
}
