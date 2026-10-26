using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 放置点。挂在已有 UI 上，按 groupId 接收同组 DragSource。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DragPlace : MonoBehaviour {
        [FoldoutGroup("拖拽")]
        [LabelText("允许放置")]
        [SerializeField]
        bool _enablePlace = true;

        [FoldoutGroup("拖拽")]
        [LabelText("组 ID")]
        [SerializeField]
        string _groupId = "";

        [FoldoutGroup("拖拽")]
        [LabelText("控制器(空则自动查找)")]
        [SerializeField]
        DragController _assignedController;

        Func<DragSource, bool> _canAccept;
        DragController _owner;
        RectTransform _rect;
        bool _isClone;
        bool _warnedNoController;

        /// <summary>是否参与放置判定。</summary>
        public bool EnablePlace {
            get => _enablePlace;
            set => _enablePlace = value;
        }

        /// <summary>分组；须与拖拽源的 groupId 一致。</summary>
        public string GroupId => _groupId ?? string.Empty;

        /// <summary>当前登记到的控制器。</summary>
        public DragController Owner => _owner;

        /// <summary>自身矩形。没有 RectTransform 时为 null。</summary>
        public RectTransform RectTransform {
            get {
                if (_rect == null) {
                    _rect = transform as RectTransform;
                }

                return _rect;
            }
        }

        public event Action<DragEventArgs> Entered;

        public event Action<DragEventArgs> Exited;

        public event Action<DragEventArgs> DragSucceeded;

        public event Action<DragEventArgs> DragFailed;

        // 额外接收条件。返回 false 时本次不能放下。
        public void SetCanAccept(Func<DragSource, bool> predicate) {
            _canAccept = predicate;
        }

        // 修改组 ID，已登记时同步到控制器。
        public void SetGroupId(string groupId) {
            _groupId = groupId ?? string.Empty;
            if (_owner != null) {
                _owner.RefreshPlaceGroup(this);
            }
        }

        // 指定控制器。传 null 则重新自动查找。
        public void SetController(DragController controller) {
            _assignedController = controller;
            if (Application.isPlaying == false || isActiveAndEnabled == false) {
                return;
            }

            Rebind();
        }

        // 当前是否接收这个源。分组由控制器判断，这里只看开关和谓词。
        public bool CanAccept(DragSource source) {
            if (_enablePlace == false || isActiveAndEnabled == false || source == null) {
                return false;
            }

            if (_canAccept != null && _canAccept.Invoke(source) == false) {
                return false;
            }

            return true;
        }

        // 屏幕点是否落在放置矩形内。
        public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera) {
            if (RectTransform == null) {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(RectTransform, screenPoint, eventCamera);
        }

        // 副本上的放置点不再登记。
        internal void MarkAsClone() {
            _isClone = true;
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

        // 控制器改组时写回字段。
        internal void ApplyGroupId(string groupId) {
            _groupId = groupId ?? string.Empty;
        }

        // 派发指针进入。
        internal void RaiseEntered(DragEventArgs args) {
            Action<DragEventArgs> handler = Entered;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 派发指针离开。
        internal void RaiseExited(DragEventArgs args) {
            Action<DragEventArgs> handler = Exited;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 派发放置成功。
        internal void RaiseSucceeded(DragEventArgs args) {
            Action<DragEventArgs> handler = DragSucceeded;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 派发放置失败。
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

            Rebind();
        }

        // 禁用时解除登记。
        void OnDisable() {
            if (_isClone == true || DragCloneGuard.IsCloning == true) {
                return;
            }

            if (_owner != null) {
                _owner.UnregisterPlace(this);
            }
        }

        // 换父节点后重新查找控制器。
        void OnTransformParentChanged() {
            if (_isClone == true || DragCloneGuard.IsCloning == true) {
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
            if (Application.isPlaying == true && _owner != null) {
                _owner.RefreshPlaceGroup(this);
            }
        }
#endif

        // 按指定控制器、父节点或 Canvas 重新登记。
        void Rebind() {
            DragController next = DragControllerLookup.Resolve(this, _assignedController);
            if (next == _owner) {
                // 还是原来的控制器，只刷新分组。
                if (next != null) {
                    next.RefreshPlaceGroup(this);
                }
            }
            else if (next == null) {
                // 换了位置但找不到新控制器。
                if (_owner != null) {
                    _owner.UnregisterPlace(this);
                }

                WarnNoController();
            }
            else {
                // 登记到新的控制器。
                if (_owner != null) {
                    _owner.UnregisterPlace(this);
                }

                next.RegisterPlace(this);
            }
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
