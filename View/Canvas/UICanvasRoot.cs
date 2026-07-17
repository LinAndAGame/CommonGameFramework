using System.Collections.Generic;
using UnityEngine;

namespace CommonGameFramework.View
{
    /// <summary>
    /// 多 Canvas 根节点；游戏层在 Init 时 RegisterCanvas 绑定 ViewLayer 子类与 Canvas。
    /// </summary>
    public class UICanvasRoot : MonoBehaviour
    {
        readonly Dictionary<ViewLayer, Canvas> _layerMap = new Dictionary<ViewLayer, Canvas>();

        /// <summary>注册 ViewLayer 实例与对应 Canvas。</summary>
        public void RegisterCanvas(ViewLayer layer, Canvas canvas)
        {
            if (layer == null || canvas == null) return;
            _layerMap[layer] = canvas;
        }

        public void UnregisterCanvas(ViewLayer layer)
        {
            if (layer == null) return;
            _layerMap.Remove(layer);
        }

        public Transform GetLayerRoot(ViewLayer layer)
        {
            if (layer != null && _layerMap.TryGetValue(layer, out var canvas) && canvas != null)
                return canvas.transform;

            return transform;
        }
    }
}
