using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 当前拖拽的控制器。Source / Place 按 groupId 登记到这里。
    /// 没放控制器时，运行时会在 Canvas 下创建名为 [DragPlace] 的节点。
    /// 失败回源用 DOTween，等待完成用 UniTask。框架依赖这两项插件。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class DragController : MonoBehaviour {
        [FoldoutGroup("拖拽")]
        [LabelText("启用拖拽")]
        [SerializeField]
        bool _enableDragSystem = true;

        [FoldoutGroup("拖拽")]
        [LabelText("拖拽层(空则用 Canvas)")]
        [SerializeField]
        RectTransform _dragLayer;

        [FoldoutGroup("拖拽")]
        [LabelText("拖拽配置")]
        [SerializeField]
        DragSettings _settings;

        readonly DragRegistry _registry = new DragRegistry();
        CurrentDrag _currentDrag;
        int _trackedCanvasId;
        bool _autoCreated;
        bool _isClone;
        Func<DragSource, DragPlace, bool> _canPlace;

        /// <summary>总开关。关掉后不能开始新的拖拽。</summary>
        public bool EnableDragSystem {
            get => _enableDragSystem;
            set => _enableDragSystem = value;
        }

        /// <summary>跟随物的父节点。空则用所在 Canvas。</summary>
        public RectTransform DragLayer {
            get => _dragLayer;
            set => _dragLayer = value;
        }

        /// <summary>配置资产。空表示用 DragSettings 的默认值。</summary>
        public DragSettings Settings {
            get => _settings;
            set => _settings = value;
        }

        public event Action<DragEventArgs> PlaceEntered;

        public event Action<DragEventArgs> PlaceExited;

        public event Action<DragEventArgs> DragSucceeded;

        public event Action<DragEventArgs> DragFailed;

        // 未指定资产时用运行时默认配置。
        internal DragSettings EffectiveSettings => DragSettingsHelper.Resolve(_settings);

        // 克隆体不参加查找。
        internal bool IsClone => _isClone;

        // 登记拖拽源，组 ID 取源上的 GroupId。
        public void RegisterSource(DragSource source) {
            if (source == null || _isClone == true) {
                return;
            }

            DragController previous = source.Owner;
            if (previous != null && previous != this) {
                previous.UnregisterSource(source);
            }

            _registry.TrackSource(source, source.GroupId);
            source.Attach(this);
        }

        // 解除拖拽源。若它正在被拖，会立刻还原。
        public void UnregisterSource(DragSource source) {
            if (source == null) {
                return;
            }

            bool tracked = _registry.ContainsSource(source);
            if (tracked == false && source.Owner != this) {
                return;
            }

            _registry.UntrackSource(source);
            if (tracked == true) {
                CurrentDrag.NotifySourceRemoved(source);
            }

            source.Detach(this);
        }

        // 把已登记的源改到另一组。
        public bool SetSourceGroup(DragSource source, string groupId) {
            if (source == null || _registry.ContainsSource(source) == false) {
                return false;
            }

            source.ApplyGroupId(groupId);
            _registry.TrackSource(source, source.GroupId);
            return true;
        }

        // 读取源所在组。未登记时返回 null。
        public string GetSourceGroupId(DragSource source) {
            if (_registry.TryGetSourceGroup(source, out string groupId) == false) {
                return null;
            }

            return groupId;
        }

        // 该组已登记的源。不要修改返回的列表。
        public IReadOnlyList<DragSource> GetSources(string groupId) {
            return _registry.GetSources(groupId);
        }

        // 登记放置点。
        public void RegisterPlace(DragPlace place) {
            if (place == null || _isClone == true) {
                return;
            }

            DragController previous = place.Owner;
            if (previous != null && previous != this) {
                previous.UnregisterPlace(place);
            }

            _registry.TrackPlace(place, place.GroupId);
            place.Attach(this);
        }

        // 解除放置点。
        public void UnregisterPlace(DragPlace place) {
            if (place == null) {
                return;
            }

            bool tracked = _registry.ContainsPlace(place);
            if (tracked == false && place.Owner != this) {
                return;
            }

            _registry.UntrackPlace(place);
            if (tracked == true) {
                CurrentDrag.NotifyPlaceRemoved(place);
            }

            place.Detach(this);
        }

        // 把已登记的放置点改到另一组。
        public bool SetPlaceGroup(DragPlace place, string groupId) {
            if (place == null || _registry.ContainsPlace(place) == false) {
                return false;
            }

            place.ApplyGroupId(groupId);
            _registry.TrackPlace(place, place.GroupId);
            return true;
        }

        // 读取放置点所在组。未登记时返回 null。
        public string GetPlaceGroupId(DragPlace place) {
            if (_registry.TryGetPlaceGroup(place, out string groupId) == false) {
                return null;
            }

            return groupId;
        }

        // 该组已登记的放置点。不要修改返回的列表。
        public IReadOnlyList<DragPlace> GetPlaces(string groupId) {
            return _registry.GetPlaces(groupId);
        }

        // 控制器级放置条件，与 DragPlace.CanAccept 同时生效。
        public void SetCanPlaceHandler(Func<DragSource, DragPlace, bool> handler) {
            _canPlace = handler;
        }

        // 开始拖拽。自定义指针也可以直接调用。
        public void HandleBeginDrag(DragSource source, PointerEventData eventData) {
            CurrentDrag.Begin(source, eventData);
        }

        // 拖拽移动。
        public void HandleDrag(DragSource source, PointerEventData eventData) {
            CurrentDrag.Drag(source, eventData);
        }

        // 结束拖拽。
        public void HandleEndDrag(DragSource source, PointerEventData eventData) {
            CurrentDrag.End(source, eventData);
        }

        // 立刻结束当前拖拽或回源，视觉回到拖拽前。
        public void StopDrag() {
            if (_currentDrag == null) {
                return;
            }

            _currentDrag.ForceStop();
        }

        // 源的组 ID 已在组件上改过，这里只刷新索引。
        internal void RefreshSourceGroup(DragSource source) {
            if (source == null || _registry.ContainsSource(source) == false) {
                return;
            }

            _registry.TrackSource(source, source.GroupId);
        }

        // 放置点的组 ID 已在组件上改过，这里只刷新索引。
        internal void RefreshPlaceGroup(DragPlace place) {
            if (place == null || _registry.ContainsPlace(place) == false) {
                return;
            }

            _registry.TrackPlace(place, place.GroupId);
        }

        // 自动创建的控制器用自身矩形当拖拽层。
        internal void UseDragLayer(RectTransform layer) {
            if (_dragLayer == null) {
                _dragLayer = layer;
            }
        }

        // 标记为运行时自动创建，开始拖拽时把层拿到同级最后。
        internal void MarkAutoCreated() {
            _autoCreated = true;
        }

        // 克隆体不参加静态登记。
        internal void MarkAsClone() {
            _isClone = true;
        }

        // 组件启用且总开关打开才能开始拖拽。
        internal bool AllowsDrag() {
            if (_enableDragSystem == false || isActiveAndEnabled == false) {
                return false;
            }

            return true;
        }

        // 放置点自身条件和控制器条件都要通过。
        internal bool EvaluateCanPlace(DragSource source, DragPlace place) {
            if (place == null || place.CanAccept(source) == false) {
                return false;
            }

            if (_canPlace != null && _canPlace.Invoke(source, place) == false) {
                return false;
            }

            return true;
        }

        // 解析跟随物的父节点。自动创建的层每次拖拽都拿到最前。
        internal RectTransform ResolveDragLayer() {
            RectTransform layer = _dragLayer;
            if (layer == null) {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) {
                    layer = canvas.transform as RectTransform;
                }
            }

            if (layer == null) {
                layer = transform as RectTransform;
            }

            if (_autoCreated == true && layer != null) {
                layer.SetAsLastSibling();
            }

            return layer;
        }

        // 派发进入放置点。
        internal void RaisePlaceEntered(DragEventArgs args) {
            Action<DragEventArgs> handler = PlaceEntered;
            if (handler != null) {
                handler.Invoke(args);
            }
        }

        // 派发离开放置点。
        internal void RaisePlaceExited(DragEventArgs args) {
            Action<DragEventArgs> handler = PlaceExited;
            if (handler != null) {
                handler.Invoke(args);
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

        // 第一次拖拽时创建当前这一次拖拽。
        CurrentDrag CurrentDrag {
            get {
                if (_currentDrag == null) {
                    _currentDrag = new CurrentDrag(this, _registry);
                }

                return _currentDrag;
            }
        }

        // 进入可被查找的控制器列表，并记住所在 Canvas。
        void OnEnable() {
            if (_isClone == true || DragCloneGuard.IsCloning == true) {
                return;
            }

            if (DragControllerHelper.Active.Contains(this) == false) {
                DragControllerHelper.Active.Add(this);
            }

            RememberCanvas();
        }

        // 停止当前拖拽，并从查找表里移除。已登记的源和放置点先留着，便于再次启用。
        void OnDisable() {
            if (_isClone == true || DragCloneGuard.IsCloning == true) {
                return;
            }

            DragControllerHelper.SuspendAutoCreate = true;
            try {
                if (_currentDrag != null) {
                    _currentDrag.ForceStop();
                }

                DragControllerHelper.Active.Remove(this);
                ForgetCanvas();
            }
            finally {
                DragControllerHelper.SuspendAutoCreate = false;
            }
        }

        // 销毁时解开仍登记着的源和放置点。
        void OnDestroy() {
            if (_isClone == true) {
                return;
            }

            DragControllerHelper.SuspendAutoCreate = true;
            try {
                if (_currentDrag != null) {
                    _currentDrag.ForceStop();
                }

                DetachMembers();
                DragControllerHelper.Active.Remove(this);
                ForgetCanvas();
            }
            finally {
                DragControllerHelper.SuspendAutoCreate = false;
            }
        }

        // 把登记关系从组件上摘掉，不在销毁过程中新建控制器。
        void DetachMembers() {
            List<DragSource> sources = _registry.SnapshotSources();
            for (int i = 0; i < sources.Count; i++) {
                DragSource source = sources[i];
                _registry.UntrackSource(source);
                if (source != null) {
                    source.Detach(this);
                    source.SetDragLocked(false);
                }
            }

            List<DragPlace> places = _registry.SnapshotPlaces();
            for (int i = 0; i < places.Count; i++) {
                DragPlace place = places[i];
                _registry.UntrackPlace(place);
                if (place != null) {
                    place.Detach(this);
                }
            }
        }

        // 记录所在 Canvas，不覆盖已经记下的另一台控制器。
        void RememberCanvas() {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) {
                return;
            }

            int id = canvas.GetInstanceID();
            if (DragControllerHelper.ByCanvas.TryGetValue(id, out DragController existing) == true && existing != null && existing != this) {
                return;
            }

            DragControllerHelper.ByCanvas[id] = this;
            _trackedCanvasId = id;
        }

        // 只移除自己写下的 Canvas 记录。
        void ForgetCanvas() {
            if (_trackedCanvasId == 0) {
                return;
            }

            if (DragControllerHelper.ByCanvas.TryGetValue(_trackedCanvasId, out DragController existing) == true && existing == this) {
                DragControllerHelper.ByCanvas.Remove(_trackedCanvasId);
            }

            _trackedCanvasId = 0;
        }

    }
}
