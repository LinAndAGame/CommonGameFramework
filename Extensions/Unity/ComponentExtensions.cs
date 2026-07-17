using UnityEngine;

namespace CommonGameFramework.Extensions
{
    /// <summary>Component 扩展。</summary>
    public static class ComponentExtensions
    {
        /// <summary>获取组件，缺失时 LogError 并返回 null（不 AddComponent）。</summary>
        public static T GetComponentOrLogError<T>(this Component component) where T : Component
        {
            if (component == null) return null;
            var found = component.GetComponent<T>();
            if (found == null)
                Debug.LogError($"[Component] {component.name} 缺少组件 {typeof(T).Name}。", component);
            return found;
        }

        public static T GetComponentInChildrenOrLogError<T>(this Component component, bool includeInactive = false)
            where T : Component
        {
            if (component == null) return null;
            var found = component.GetComponentInChildren<T>(includeInactive);
            if (found == null)
                Debug.LogError($"[Component] {component.name} 子层级缺少组件 {typeof(T).Name}。", component);
            return found;
        }
    }
}
