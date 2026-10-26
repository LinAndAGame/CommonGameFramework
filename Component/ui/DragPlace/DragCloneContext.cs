using UnityEngine;

namespace CommonGameFramework.DragPlace {
    /// <summary>创建拖拽副本时交给 IDragClone 的上下文。以后加字段不影响接口方法签名。</summary>
    public sealed class DragCloneContext {
        // 记下这一次要复制的源、原件和拖拽层。
        public DragCloneContext(DragSource source, RectTransform origin, RectTransform dragLayer) {
            Source = source;
            Origin = origin;
            DragLayer = dragLayer;
        }

        /// <summary>正在拖的源。</summary>
        public DragSource Source { get; }

        /// <summary>被复制的原件。通常是 DragSource.DragTarget。</summary>
        public RectTransform Origin { get; }

        /// <summary>副本要挂到的拖拽层。</summary>
        public RectTransform DragLayer { get; }
    }
}
