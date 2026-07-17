using System;
using System.Collections.Generic;

namespace CommonGameFramework.Pool
{
    internal sealed class ClassPool<T> : IClassPool where T : class, IPoolable, new()
    {
        readonly Stack<T> _inactive = new Stack<T>();
        readonly HashSet<T> _active = new HashSet<T>();
        int _totalCreated;

        public T Get()
        {
            if (_inactive.Count == 0)
            {
                var created = new T();
                _totalCreated++;
                _inactive.Push(created);
            }

            var item = _inactive.Pop();
            item.OnSpawnFromPool();
            _active.Add(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            if (_active.Remove(item) == false)
            {
                throw new ArgumentException($"对象 {item} 不属于 {typeof(T).Name} 对象池。", nameof(item));
            }

            item.OnReturnToPool();
            _inactive.Push(item);
        }

        void IClassPool.Release(IPoolable item)
        {
            if (item is T typed)
            {
                Release(typed);
                return;
            }

            throw new ArgumentException($"对象类型 {item.GetType().Name} 与池类型 {typeof(T).Name} 不匹配。", nameof(item));
        }

        public void Preload(int count)
        {
            if (count <= 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                var created = new T();
                _totalCreated++;
                _inactive.Push(created);
            }
        }

        public PoolSnapshot GetSnapshot()
        {
            return new PoolSnapshot(typeof(T).Name, PoolKind.CSharp, _inactive.Count, _active.Count, _totalCreated, null);
        }
    }
}
