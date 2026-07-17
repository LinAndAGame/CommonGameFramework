using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// Mono 对象池基础设施组件，挂在 prefab 根节点。
    /// </summary>
    public sealed class MonoPoolable : MonoBehaviour, IPoolable
    {
        [ShowInInspector, ReadOnly, LabelText("来源预制体")]
        GameObject _sourcePrefab;

        public GameObject SourcePrefab => _sourcePrefab;
        public Transform PoolRoot { get; private set; }

        internal void Bind(GameObject sourcePrefab, Transform poolRoot)
        {
            _sourcePrefab = sourcePrefab;
            PoolRoot = poolRoot;
        }

        public void OnSpawnFromPool()
        {
            NotifyOtherPoolables(spawn: true);
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            NotifyOtherPoolables(spawn: false);
            gameObject.SetActive(false);
        }

        void NotifyOtherPoolables(bool spawn)
        {
            var poolables = GetComponents<IPoolable>();
            for (var i = 0; i < poolables.Length; i++)
            {
                var poolable = poolables[i];
                if (ReferenceEquals(poolable, this) == true)
                {
                    continue;
                }

                if (spawn == true)
                {
                    poolable.OnSpawnFromPool();
                }
                else
                {
                    poolable.OnReturnToPool();
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying == false && GetComponents<IPoolable>().Length <= 1)
            {
                Debug.LogWarning($"[{nameof(MonoPoolable)}] {name} 建议在同 GameObject 上挂载实现 {nameof(IPoolable)} 的业务脚本。", this);
            }
        }
#endif
    }
}
