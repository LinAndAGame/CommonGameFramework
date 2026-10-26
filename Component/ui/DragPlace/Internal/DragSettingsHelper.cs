using UnityEngine;

namespace CommonGameFramework.DragPlace {
    /// <summary>空的 DragSettings 用一份隐藏的运行时默认值补上。</summary>
    internal static class DragSettingsHelper {
        static DragSettings _runtimeDefault;

        // 已传入配置就用它，否则返回隐藏的默认实例。
        internal static DragSettings Resolve(DragSettings settings) {
            DragSettings resolved = settings;
            if (settings != null) {
                // 调用方已经给了配置。
                resolved = settings;
            }
            else if (_runtimeDefault != null) {
                // 沿用已经建好的默认实例。
                resolved = _runtimeDefault;
            }
            else {
                // 第一次使用时创建一份不进场景的默认配置。
                _runtimeDefault = ScriptableObject.CreateInstance<DragSettings>();
                _runtimeDefault.name = "DragSettingsHelperDefault";
                _runtimeDefault.hideFlags = HideFlags.HideAndDontSave;
                resolved = _runtimeDefault;
            }

            return resolved;
        }

        // 域重载时丢掉默认实例，下一轮再按字段默认值重建。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetRuntimeDefault() {
            _runtimeDefault = null;
        }
    }
}
