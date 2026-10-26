using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 把子节点上的拖拽接口转给 DragSource。
    /// 射线目标自己带有拖拽接口、事件到不了父节点上的 DragSource 时使用。
    /// 与 DragSource 挂在同一物体上时不转发，避免一次拖拽触发两次。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DragPointerBridge : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [LabelText("拖拽源(空则父节点)")]
        [SerializeField]
        DragSource _source;

        // 改绑要转发的源。
        public void Bind(DragSource source) {
            _source = source;
        }

        // 开始拖拽时转给源。
        public void OnBeginDrag(PointerEventData eventData) {
            DragSource source = ResolveSource();
            if (source == null || source.gameObject == gameObject) {
                return;
            }

            source.ProcessBeginDrag(eventData);
        }

        // 拖拽移动时转给源。
        public void OnDrag(PointerEventData eventData) {
            DragSource source = ResolveSource();
            if (source == null || source.gameObject == gameObject) {
                return;
            }

            source.ProcessDrag(eventData);
        }

        // 结束拖拽时转给源。
        public void OnEndDrag(PointerEventData eventData) {
            DragSource source = ResolveSource();
            if (source == null || source.gameObject == gameObject) {
                return;
            }

            source.ProcessEndDrag(eventData);
        }

        // 优先用序列化引用，其次自身，再向父节点找。
        DragSource ResolveSource() {
            DragSource source = _source;
            if (_source != null) {
                // 用已经绑好的源。
                source = _source;
            }
            else {
                // 先看自身，再向父节点找。
                source = GetComponent<DragSource>();
                if (source == null) {
                    source = GetComponentInParent<DragSource>();
                }

                _source = source;
            }

            return source;
        }
    }
}
