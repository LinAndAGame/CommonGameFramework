using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 拖拽源。挂在已有 UI 上，按 groupId 向 DragController 注册，不要求游戏中间层。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
        [FoldoutGroup("拖拽")]
        [LabelText("允许拖拽")]
        [SerializeField]
        bool _enableDrag = true;

        [FoldoutGroup("拖拽")]
        [LabelText("拖拽对象(空则自身)")]
        [SerializeField]
        RectTransform _dragTarget;

        [FoldoutGroup("拖拽")]
        [LabelText("组 ID")]
        [SerializeField]
        string _groupId = "";

        [FoldoutGroup("拖拽")]
        [LabelText("控制器(空则自动查找)")]
        [SerializeField]
        DragController _assignedController;

        DragController _owner;
        bool _dragLocked;
        bool _isClone;
        bool _warnedGraphic;
        bool _warnedNoController;

        /// <summary>是否允许开始拖拽。</summary>
        public bool EnableDrag {
            get => _enableDrag;
            set => _enableDrag = value;
        }

        /// <summary>分组；与放置点字符串完全一致才能配对。空字符串是默认组。</summary>
        public string GroupId => _groupId ?? string.Empty;

        /// <summary>业务数据，会放进 DragEventArgs.UserData。</summary>
        public object UserData { get; private set; }

        /// <summary>当前登记到的控制器。</summary>
        public DragController Owner => _owner;

        public event Action<PointerEventData> BeginDrag;

        public event Action<PointerEventData> Dragging;

        public event Action<PointerEventData> EndDrag;

        public event Action<DragEventArgs> DragSucceeded;

        public event Action<DragEventArgs> DragFailed;

        /// <summary>跟随指针的矩形。未指定时用自身。</summary>
        public RectTransform DragTarget {
            get {
                if (_dragTarget != null) {
                    return _dragTarget;
                }

                return transform as RectTransform;
            }
        }

        // 写入业务数据。
        public void SetUserData(object data) {
            UserData = data;
        }

        // 修改组 ID，已登记时同步到控制器。
        public void SetGroupId(string groupId) {
            _groupId = groupId ?? string.Empty;
            if (_owner != null) {
                _owner.RefreshSourceGroup(this);
            }
        }

        // 指定被拖动的矩形。传 null 则回到自身。
        public void SetDragTarget(RectTransform target) {
            _dragTarget = target;
        }

        // 指定控制器。传 null 则重新自动查找。
        public void SetController(DragController controller) {
            _assignedController = controller;
            if (Application.isPlaying == false || isActiveAndEnabled == false) {
                return;
            }

            Rebind();
        }

        // 由 EventSystem 或外部指针开始拖拽。
        public void ProcessBeginDrag(PointerEventData eventData) {
            if (_owner == null) {
                Rebind();
            }

            if (_owner == null) {
                WarnNoController();
                return;
            }

            _owner.HandleBeginDrag(this, eventData);
        }

        // 拖拽移动。
        public void ProcessDrag(PointerEventData eventData) {
            if (_owner == null) {
                return;
            }

            _owner.HandleDrag(this, eventData);
        }

        // 结束拖拽。
        public void ProcessEndDrag(PointerEventData eventData) {
            if (_owner == null) {
                return;
            }

            _owner.HandleEndDrag(this, eventData);
        }

        // EventSystem 按下并拖动。
        public void OnBeginDrag(PointerEventData eventData) {
            ProcessBeginDrag(eventData);
        }

        // EventSystem 拖动中。
        public void OnDrag(PointerEventData eventData) {
            ProcessDrag(eventData);
        }

        // EventSystem 松开。
        public void OnEndDrag(PointerEventData eventData) {
            ProcessEndDrag(eventData);
        }

        // 副本上的源不再向控制器登记。
        internal void MarkAsClone() {
            _isClone = true;
        }

        // 拖拽改父节点期间不要因为 OnTransformParentChanged 重新登记。
        internal void SetDragLocked(bool locked) {
            _dragLocked = locked;
        }

        // 这一次拖拽结束后按当前位置重新查找控制器。
        internal void RebindIfPlaying() {
            if (_isClone == true || _dragLocked == true) {
                return;
            }

            if (Application.isPlaying == false || isActiveAndEnabled == false) {
                return;
            }

            Rebind();
        }

        // 绑到控制器。
        internal void Attach(DragController owner) {
            _owner = owner;
        }

        // 从控制器解开。
        internal void Detach(DragController owner) {
            if (_owner == owner) {
                _owner = null;
            }
        }

        // 控制器改组时写回字段，避免再次触发登记。
        internal void ApplyGroupId(string groupId) {
            _groupId = groupId ?? string.Empty;
        }

        // 派发开始拖拽。
        internal void RaiseBeginDrag(PointerEventData eventData) {
            Action<PointerEventData> handler = BeginDrag;
            if (handler != null) {
                handler.Invoke(eventData);
            }
        }

        // 派发拖拽移动。
        internal void RaiseDragging(PointerEventData eventData) {
            Action<PointerEventData> handler = Dragging;
            if (handler != null) {
                handler.Invoke(eventData);
            }
        }

        // 派发结束拖拽。
        internal void RaiseEndDrag(PointerEventData eventData) {
            Action<PointerEventData> handler = EndDrag;
            if (handler != null) {
                handler.Invoke(eventData);
            }
        }

        // 派发拖拽成功。
        internal void RaiseSucceeded(DragEventArgs args) {
            Action<DragEventArgs> handler = DragSucceeded;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 派发拖拽失败。
        internal void RaiseFailed(DragEventArgs args) {
            Action<DragEventArgs> handler = DragFailed;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 启用时向控制器登记。
        void OnEnable() {
            if (_isClone == true || DragCloneGuard.IsCloning == true || Application.isPlaying == false) {
                return;
            }

            WarnIfNoGraphic();
            Rebind();
        }

        // 禁用时解除登记。
        void OnDisable() {
            if (_isClone == true || DragCloneGuard.IsCloning == true) {
                return;
            }

            if (_owner != null) {
                _owner.UnregisterSource(this);
            }
        }

        // 换父节点后，若不是正在拖拽改父，重新查找控制器。
        void OnTransformParentChanged() {
            if (_isClone == true || _dragLocked == true || DragCloneGuard.IsCloning == true) {
                return;
            }

            if (Application.isPlaying == false || isActiveAndEnabled == false) {
                return;
            }

            Rebind();
        }

#if UNITY_EDITOR
        // 运行中在 Inspector 改组 ID 时同步登记。
        void OnValidate() {
            if (Application.isPlaying == true && _owner != null && _dragLocked == false) {
                _owner.RefreshSourceGroup(this);
            }
        }
#endif

        // 按指定控制器、父节点或 Canvas 重新登记。
        void Rebind() {
            DragController next = DragControllerHelper.Resolve(this, _assignedController);
            if (next == _owner) {
                // 还是原来的控制器，只刷新分组。
                if (next != null) {
                    next.RefreshSourceGroup(this);
                }
            }
            else if (next == null) {
                // 换了位置但找不到新控制器。
                if (_owner != null) {
                    _owner.UnregisterSource(this);
                }

                WarnNoController();
            }
            else {
                // 登记到新的控制器。
                if (_owner != null) {
                    _owner.UnregisterSource(this);
                }

                next.RegisterSource(this);
            }
        }

        // 没有 Graphic 时射线打不中这个源。
        void WarnIfNoGraphic() {
            if (_warnedGraphic == true) {
                return;
            }

            if (GetComponent<Graphic>() != null) {
                return;
            }

            if (GetComponentInChildren<Graphic>(true) != null) {
                return;
            }

            _warnedGraphic = true;
            Debug.LogWarning($"[DragPlace] {name} 缺少 Graphic，射线无法开始拖拽。", this);
        }

        // 找不到控制器时提示一次。
        void WarnNoController() {
            if (_warnedNoController == true) {
                return;
            }

            _warnedNoController = true;
            Debug.LogWarning($"[DragPlace] {name} 没有 DragController，也不在 Canvas 下。", this);
        }
    }
}
