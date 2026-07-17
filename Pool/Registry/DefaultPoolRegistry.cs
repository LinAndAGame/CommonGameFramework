using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 默认 IPoolRegistry 实现。
    /// </summary>
    public sealed class DefaultPoolRegistry : IPoolRegistry
    {
        readonly Dictionary<Type, IClassPool> _classPools = new Dictionary<Type, IClassPool>();
        readonly Dictionary<GameObject, MonoPool> _monoPools = new Dictionary<GameObject, MonoPool>();
        readonly Dictionary<Component, IComponentPool> _componentPools = new Dictionary<Component, IComponentPool>();
        readonly HashSet<GameObject> _monoPoolsViaComponent = new HashSet<GameObject>();

        public T Get<T>() where T : class, IPoolable, new() => GetClassPool<T>().Get();

        public void Release(IPoolable item)
        {
            if (item == null) return;

            if (item is Component component)
            {
                ReleaseComponent(component);
                return;
            }

            if (_classPools.TryGetValue(item.GetType(), out var pool) == false)
                throw new ArgumentException($"类型 {item.GetType().Name} 未注册对象池。", nameof(item));

            pool.Release(item);
        }

        public void Preload<T>(int count = 1) where T : class, IPoolable, new()
        {
            GetClassPool<T>().Preload(count);
        }

        public GameObject Get(GameObject prefab)
        {
            ValidatePrefab(prefab);
            return GetMonoPool(prefab).Get();
        }

        public T Get<T>(T prefab) where T : Component
        {
            ValidateComponentPrefab(prefab);
            return GetComponentPool(prefab).Get();
        }

        public T Get<T>(GameObject prefab) where T : Component
        {
            ValidatePrefab(prefab);
            var prefabComponent = prefab.GetComponent<T>();
            if (prefabComponent != null)
                return Get(prefabComponent);

            var instance = Get(prefab);
            var component = instance.GetComponent<T>();
            if (component == null)
                Debug.LogError($"[Pool] 实例 {instance.name} 缺少组件 {typeof(T).Name}。", instance);

            return component;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            var poolable = instance.GetComponent<MonoPoolable>();
            if (poolable == null || poolable.SourcePrefab == null)
            {
                Debug.LogError($"[Pool] 无法归还实例 {instance.name}：缺少 {nameof(MonoPoolable)} 或 SourcePrefab。", instance);
                Object.Destroy(instance);
                return;
            }

            if (_monoPools.TryGetValue(poolable.SourcePrefab, out var pool) == false)
            {
                Debug.LogError($"[Pool] 未找到 {poolable.SourcePrefab.name} 对应的对象池。", instance);
                Object.Destroy(instance);
                return;
            }

            pool.Release(instance);
        }

        public void Preload(GameObject prefab, int count = 1)
        {
            ValidatePrefab(prefab);
            GetMonoPool(prefab).Preload(count);
        }

        public void Preload<T>(T prefab, int count = 1) where T : Component
        {
            ValidateComponentPrefab(prefab);
            GetComponentPool(prefab).Preload(count);
        }

        public void Preload<T>(GameObject prefab, int count = 1) where T : Component
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

        public IReadOnlyList<PoolSnapshot> GetSnapshots()
        {
            var snapshots = new List<PoolSnapshot>(_classPools.Count + _monoPools.Count + _componentPools.Count);

            foreach (var pool in _classPools.Values)
                snapshots.Add(pool.GetSnapshot());

            foreach (var pair in _monoPools)
            {
                if (_monoPoolsViaComponent.Contains(pair.Key)) continue;
                snapshots.Add(pair.Value.GetSnapshot());
            }

            foreach (var pool in _componentPools.Values)
                snapshots.Add(pool.GetSnapshot());

            return snapshots;
        }

        public void Reset()
        {
            _classPools.Clear();
            _monoPools.Clear();
            _componentPools.Clear();
            _monoPoolsViaComponent.Clear();
        }

        ClassPool<T> GetClassPool<T>() where T : class, IPoolable, new()
        {
            var type = typeof(T);
            if (_classPools.TryGetValue(type, out var pool) == false)
            {
                pool = new ClassPool<T>();
                _classPools[type] = pool;
            }

            return (ClassPool<T>)pool;
        }

        MonoPool GetMonoPool(GameObject prefab)
        {
            if (_monoPools.TryGetValue(prefab, out var pool) == false)
            {
                var root = PoolRuntime.CreatePoolRoot(prefab);
                pool = new MonoPool(prefab, root);
                _monoPools[prefab] = pool;
            }

            return pool;
        }

        ComponentPool<T> GetComponentPool<T>(T prefab) where T : Component
        {
            if (_componentPools.TryGetValue(prefab, out var pool) == false)
            {
                var monoPool = GetMonoPool(prefab.gameObject);
                pool = new ComponentPool<T>(prefab, monoPool);
                _componentPools[prefab] = pool;
                _monoPoolsViaComponent.Add(prefab.gameObject);
            }

            return (ComponentPool<T>)pool;
        }

        void ReleaseComponent(Component component)
        {
            var poolable = component.GetComponent<MonoPoolable>();
            if (poolable == null || poolable.SourcePrefab == null)
            {
                Debug.LogError($"[Pool] 无法归还实例 {component.name}：缺少 {nameof(MonoPoolable)} 或 SourcePrefab。", component);
                Object.Destroy(component.gameObject);
                return;
            }

            var sourceComponent = poolable.SourcePrefab.GetComponent(component.GetType());
            if (sourceComponent != null && _componentPools.TryGetValue(sourceComponent, out var pool))
            {
                pool.Release(component);
                return;
            }

            Release(component.gameObject);
        }

        static void ValidateComponentPrefab<T>(T prefab) where T : Component
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            ValidatePrefab(prefab.gameObject);
        }

        static void ValidatePrefab(GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            if (prefab.GetComponent<MonoPoolable>() == null)
                Debug.LogError($"[Pool] Prefab {prefab.name} 缺少 {nameof(MonoPoolable)} 组件。", prefab);
        }
    }
}
