using UnityEngine;

namespace CommonGameFramework.Pool
{
    /// <summary>
    /// Mono 对象池运行时根节点，替代场景内 PoolManager。
    /// </summary>
    internal static class PoolRuntime
    {
        static Transform _root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _root = null;
            Pool.ResetStatics();
        }

        public static Transform Root
        {
            get
            {
                if (_root != null)
                {
                    return _root;
                }

                var go = new GameObject("[PoolRoot]");
                Object.DontDestroyOnLoad(go);
                _root = go.transform;
                return _root;
            }
        }

        public static Transform CreatePoolRoot(GameObject prefab)
        {
            var poolRootGo = new GameObject($"[Pool]_{prefab.name}");
            poolRootGo.transform.SetParent(Root, false);
            return poolRootGo.transform;
        }
    }
}
