using UnityEngine;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 自定义拖拽副本的创建。挂在 DragSource 或被拖原件上。
    /// 没有这个组件时，框架用 Object.Instantiate 复制原件。
    /// </summary>
    public interface IDragClone {
        // 创建跟随副本。返回 null 表示这一次不建副本。不要返回原件自身。
        GameObject CreateClone(DragCloneContext context);
    }
}
