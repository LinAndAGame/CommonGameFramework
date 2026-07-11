using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CardMaster.Framework.Pool
{
    /// <summary>
    /// 全局静态对象池门面。
    /// </summary>
    public static class Pool
    {
        static readonly Dictionary<Type, IClassPool> ClassPools = new Dictionary<Type, IClassPool>();
        static readonly Dictionary<GameObject, MonoPool> MonoPools = new Dictionary<GameObject, MonoPool>();
        static readonly Dictionary<Component, IComponentPool> ComponentPools = new Dictionary<Component, IComponentPool>();
        static readonly HashSet<GameObject> MonoPoolsViaComponent = new HashSet<GameObject>();

        public static T Get<T>() where T : class, IPoolable, new()
        {
            return GetClassPool<T>().Get();
        }

        public static void Release(IPoolable item)
        {
            if (item == null)
            {
                return;
            }

            if (item is Component component)
            {
                ReleaseComponent(component);
                return;
            }

            if (ClassPools.TryGetValue(item.GetType(), out var pool) == false)
            {
                throw new ArgumentException($"类型 {item.GetType().Name} 未注册对象池。", nameof(item));
            }

            pool.Release(item);
        }

        public static void Preload<T>(int count = 1) where T : class, IPoolable, new()
        {
            GetClassPool<T>().Preload(count);
        }

        public static GameObject Get(GameObject prefab)
        {
            ValidatePrefab(prefab);
            return GetMonoPool(prefab).Get();
        }

        public static T Get<T>(T prefab) where T : Component
        {
            ValidateComponentPrefab(prefab);
            return GetComponentPool(prefab).Get();
        }

        public static T Get<T>(GameObject prefab) where T : Component
        {
            ValidatePrefab(prefab);
            var prefabComponent = prefab.GetComponent<T>();
            if (prefabComponent != null)
            {
                return Get(prefabComponent);
            }

            var instance = Get(prefab);
            var component = instance.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[Pool] 实例 {instance.name} 缺少组件 {typeof(T).Name}。", instance);
            }

            return component;
        }

        public static void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            var poolable = instance.GetComponent<MonoPoolable>();
            if (poolable == null || poolable.SourcePrefab == null)
            {
                Debug.LogError($"[Pool] 无法归还实例 {instance.name}：缺少 {nameof(MonoPoolable)} 或 SourcePrefab。", instance);
                Object.Destroy(instance);
                return;
            }

            if (MonoPools.TryGetValue(poolable.SourcePrefab, out var pool) == false)
            {
                Debug.LogError($"[Pool] 未找到 {poolable.SourcePrefab.name} 对应的对象池。", instance);
                Object.Destroy(instance);
                return;
            }

            pool.Release(instance);
        }

        public static void Preload(GameObject prefab, int count = 1)
        {
            ValidatePrefab(prefab);
            GetMonoPool(prefab).Preload(count);
        }

        public static void Preload<T>(T prefab, int count = 1) where T : Component
        {
            ValidateComponentPrefab(prefab);
            GetComponentPool(prefab).Preload(count);
        }

        public static void Preload<T>(GameObject prefab, int count = 1) where T : Component
        {
            ValidatePrefab(prefab);
            var component = prefab.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[Pool] Prefab {prefab.name} 缺少组件 {typeof(T).Name}。", prefab);
                Preload(prefab, count);
                return;
            }

            Preload(component, count);
        }

        public static IReadOnlyList<PoolSnapshot> GetSnapshots()
        {
            var snapshots = new List<PoolSnapshot>(ClassPools.Count + MonoPools.Count + ComponentPools.Count);

            foreach (var pool in ClassPools.Values)
            {
                snapshots.Add(pool.GetSnapshot());
            }

            foreach (var pair in MonoPools)
            {
                if (MonoPoolsViaComponent.Contains(pair.Key) == true)
                {
                    continue;
                }

                snapshots.Add(pair.Value.GetSnapshot());
            }

            foreach (var pool in ComponentPools.Values)
            {
                snapshots.Add(pool.GetSnapshot());
            }

            return snapshots;
        }

        internal static void ResetStatics()
        {
            ClassPools.Clear();
            MonoPools.Clear();
            ComponentPools.Clear();
            MonoPoolsViaComponent.Clear();
        }

        static ClassPool<T> GetClassPool<T>() where T : class, IPoolable, new()
        {
            var type = typeof(T);
            if (ClassPools.TryGetValue(type, out var pool) == false)
            {
                pool = new ClassPool<T>();
                ClassPools[type] = pool;
            }

            return (ClassPool<T>)pool;
        }

        static MonoPool GetMonoPool(GameObject prefab)
        {
            if (MonoPools.TryGetValue(prefab, out var pool) == false)
            {
                var root = PoolRuntime.CreatePoolRoot(prefab);
                pool = new MonoPool(prefab, root);
                MonoPools[prefab] = pool;
            }

            return pool;
        }

        static ComponentPool<T> GetComponentPool<T>(T prefab) where T : Component
        {
            if (ComponentPools.TryGetValue(prefab, out var pool) == false)
            {
                var monoPool = GetMonoPool(prefab.gameObject);
                pool = new ComponentPool<T>(prefab, monoPool);
                ComponentPools[prefab] = pool;
                MonoPoolsViaComponent.Add(prefab.gameObject);
            }

            return (ComponentPool<T>)pool;
        }

        static void ReleaseComponent(Component component)
        {
            var poolable = component.GetComponent<MonoPoolable>();
            if (poolable == null || poolable.SourcePrefab == null)
            {
                Debug.LogError($"[Pool] 无法归还实例 {component.name}：缺少 {nameof(MonoPoolable)} 或 SourcePrefab。", component);
                Object.Destroy(component.gameObject);
                return;
            }

            var sourceComponent = poolable.SourcePrefab.GetComponent(component.GetType());
            if (sourceComponent != null && ComponentPools.TryGetValue(sourceComponent, out var pool) == true)
            {
                pool.Release(component);
                return;
            }

            Release(component.gameObject);
        }

        static void ValidateComponentPrefab<T>(T prefab) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            ValidatePrefab(prefab.gameObject);
        }

        static void ValidatePrefab(GameObject prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (prefab.GetComponent<MonoPoolable>() == null)
            {
                Debug.LogError($"[Pool] Prefab {prefab.name} 缺少 {nameof(MonoPoolable)} 组件。", prefab);
            }
        }
    }
}
