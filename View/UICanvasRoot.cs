using System.Collections.Generic;
using UnityEngine;

namespace CardMaster.Framework.View
{
    public class UICanvasRoot : MonoBehaviour
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Canvas secondCanvas;
        [SerializeField] private Canvas thirdCanvas;
        [SerializeField] private Canvas popUpCanvas;
        [SerializeField] private Canvas tipCanvas;
        [SerializeField] private Canvas loadCanvas;

        readonly Dictionary<UILayer, Canvas> _layerMap = new Dictionary<UILayer, Canvas>();

        void Awake()
        {
            _layerMap[UILayer.Main] = mainCanvas;
            _layerMap[UILayer.Second] = secondCanvas;
            _layerMap[UILayer.Third] = thirdCanvas;
            _layerMap[UILayer.PopUp] = popUpCanvas;
            _layerMap[UILayer.Tip] = tipCanvas;
            _layerMap[UILayer.Load] = loadCanvas;
        }

        public Transform GetLayerRoot(UILayer layer)
        {
            if (_layerMap.TryGetValue(layer, out var canvas) == true && canvas != null)
                return canvas.transform;
            return transform;
        }
    }
}
