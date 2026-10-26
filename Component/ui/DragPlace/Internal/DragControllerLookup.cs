using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CommonGameFramework.DragPlace {
    /// <summary>查找或创建 DragController。静态方法放在这个 static class 里。</summary>
    internal static class DragControllerLookup {
        internal static readonly List<DragController> Active = new List<DragController>();
        internal static readonly Dictionary<int, DragController> ByCanvas = new Dictionary<int, DragController>();
        internal static bool Creating;
        internal static bool SuspendAutoCreate;

        // 域重载时清掉静态登记，避免编辑器里残留上一轮的控制器。
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() {
            Active.Clear();
            ByCanvas.Clear();
            Creating = false;
            SuspendAutoCreate = false;
        }

        // 按指定引用、父节点、Canvas 或场景里唯一控制器解析。
        internal static DragController Resolve(Component member, DragController assigned) {
            DragController found = null;
            if (assigned != null) {
                // 组件上已经指定了控制器。
                found = assigned;
            }
            else if (member == null) {
                // 没有挂点，无法继续查找。
                found = null;
            }
            else {
                found = ResolveFromHierarchy(member);
            }

            return found;
        }

        // 从父节点、Canvas 映射或自动创建里得到控制器。
        static DragController ResolveFromHierarchy(Component member) {
            DragController parent = FindInParents(member);
            Canvas canvas = FindCanvas(member);
            DragController found = null;
            if (parent != null) {
                // 父节点上已有控制器。
                found = parent;
            }
            else if (canvas != null && TryGetMapped(canvas, out DragController mapped) == true) {
                // 这个 Canvas 已经映射过控制器。
                found = mapped;
            }
            else if (Active.Count == 1) {
                // 场景里只有一个控制器。
                found = Active[0];
            }
            else if (Application.isPlaying == false || canvas == null || SuspendAutoCreate == true || Creating == true) {
                // 编辑器、没有 Canvas，或正在创建时，不再自动生成。
                found = null;
            }
            else {
                // 没有唯一控制器时，在所属 Canvas 下创建。
                if (Active.Count > 1) {
                    Debug.LogWarning("[DragPlace] 有多个 DragController，当前物体父级没有指定，已在所在 Canvas 下创建 [DragPlace]。", member);
                }

                found = CreateForCanvas(canvas);
            }

            return found;
        }

        // 沿父节点找第一个不是克隆体的控制器。
        static DragController FindInParents(Component member) {
            Transform current = member.transform;
            DragController found = null;
            while (current != null && found == null) {
                DragController controller = current.GetComponent<DragController>();
                if (controller != null && controller.IsClone == false) {
                    found = controller;
                }

                current = current.parent;
            }

            return found;
        }

        // 沿父节点找 Canvas。
        static Canvas FindCanvas(Component member) {
            Transform current = member.transform;
            Canvas found = null;
            while (current != null && found == null) {
                Canvas canvas = current.GetComponent<Canvas>();
                if (canvas != null) {
                    found = canvas;
                }

                current = current.parent;
            }

            return found;
        }

        // 读取 Canvas 上已经记下的控制器。
        static bool TryGetMapped(Canvas canvas, out DragController controller) {
            bool mapped = false;
            if (ByCanvas.TryGetValue(canvas.GetInstanceID(), out controller) == true && controller != null) {
                mapped = true;
            }
            else {
                controller = null;
                mapped = false;
            }

            return mapped;
        }

        // 在 Canvas 下创建 [DragPlace]，作为该画布的默认控制器和拖拽层。
        static DragController CreateForCanvas(Canvas canvas) {
            Creating = true;
            DragController created = null;
            try {
                GameObject go = new GameObject("[DragPlace]", typeof(RectTransform));
                go.layer = canvas.gameObject.layer;
                Transform parent = canvas.transform;
                go.transform.SetParent(parent, false);
                RectTransform rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.SetAsLastSibling();
                LayoutElement layout = go.AddComponent<LayoutElement>();
                layout.ignoreLayout = true;
                created = go.AddComponent<DragController>();
                created.MarkAutoCreated();
                created.UseDragLayer(rect);
            }
            finally {
                Creating = false;
            }

            return created;
        }
    }
}
