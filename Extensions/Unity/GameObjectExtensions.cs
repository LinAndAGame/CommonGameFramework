using UnityEngine;

namespace CommonGameFramework.Extensions
{
    /// <summary>GameObject 扩展。</summary>
    public static class GameObjectExtensions
    {
        public static void SetActiveSafe(this GameObject go, bool active)
        {
            if (go == null) return;
            if (go.activeSelf != active)
                go.SetActive(active);
        }

        /// <summary>获取组件，缺失时 LogError 并返回 null（不 AddComponent）。</summary>
        public static T GetComponentOrLogError<T>(this GameObject go) where T : Component
        {
            if (go == null) return null;
            var component = go.GetComponent<T>();
            if (component == null)
                Debug.LogError($"[GameObject] {go.name} 缺少组件 {typeof(T).Name}。", go);
            return component;
        }
    }
}
