using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardMaster.Framework.Pool
{
    /// <summary>
    /// 单个 Component Prefab 的对象池，底层复用 MonoPool。
    /// </summary>
    internal sealed class ComponentPool<T> : IComponentPool where T : Component
    {
        readonly T _prefab;
        readonly MonoPool _monoPool;

        public ComponentPool(T prefab, MonoPool monoPool)
        {
            _prefab = prefab;
            _monoPool = monoPool;
        }

        public T Get()
        {
            var instance = _monoPool.Get();
            var component = instance.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"[Pool] 实例 {instance.name} 缺少组件 {typeof(T).Name}。", instance);
            }

            return component;
        }

        public void Release(T component)
        {
            if (component == null)
            {
                return;
            }

            _monoPool.Release(component.gameObject);
        }

        void IComponentPool.Release(Component component)
        {
            if (component is T typed)
            {
                Release(typed);
                return;
            }

            throw new ArgumentException($"对象类型 {component.GetType().Name} 与池类型 {typeof(T).Name} 不匹配。", nameof(component));
        }

        public void Preload(int count)
        {
            _monoPool.Preload(count);
        }

        public PoolSnapshot GetSnapshot()
        {
            var monoSnapshot = _monoPool.GetSnapshot();
            return new PoolSnapshot(
                typeof(T).Name,
                PoolKind.Component,
                monoSnapshot.InactiveCount,
                monoSnapshot.ActiveCount,
                monoSnapshot.TotalCreated,
                _prefab.gameObject);
        }
    }
}
