using System.Collections.Generic;
using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// 单个 Prefab 的 Mono 对象池。
    /// </summary>
    internal sealed class MonoPool : IPool
    {
        readonly GameObject _prefab;
        readonly Transform _root;
        readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        readonly HashSet<GameObject> _active = new HashSet<GameObject>();
        int _totalCreated;

        public MonoPool(GameObject prefab, Transform root)
        {
            _prefab = prefab;
            _root = root;
        }

        public GameObject Get()
        {
            var instance = _inactive.Count > 0 ? _inactive.Pop() : CreateInstance();
            _active.Add(instance);

            var poolable = instance.GetComponent<MonoPoolable>();
            poolable?.OnSpawnFromPool();

            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (_active.Remove(instance) == false)
            {
                Debug.LogError($"[Pool] 实例 {instance.name} 不属于 {_prefab.name} 对象池。", instance);
                Object.Destroy(instance);
                return;
            }

            var poolable = instance.GetComponent<MonoPoolable>();
            poolable?.OnReturnToPool();

            instance.transform.SetParent(_root, false);
            _inactive.Push(instance);
        }

        public void Preload(int count)
        {
            if (count <= 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var instance = CreateInstance();
                instance.transform.SetParent(_root, false);
                _inactive.Push(instance);
            }
        }

        public PoolSnapshot GetSnapshot()
        {
            return new PoolSnapshot(_prefab.name, PoolKind.Mono, _inactive.Count, _active.Count, _totalCreated, _prefab);
        }

        GameObject CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _root);
            _totalCreated++;

            var poolable = instance.GetComponent<MonoPoolable>();
            if (poolable == null)
            {
                Debug.LogError($"[Pool] Prefab {_prefab.name} 缺少 {nameof(MonoPoolable)} 组件。", _prefab);
                poolable = instance.AddComponent<MonoPoolable>();
            }

            poolable.Bind(_prefab, _root);
            instance.SetActive(false);
            return instance;
        }
    }
}
