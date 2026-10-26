using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace CommonGameFramework.DragPlace {
    /// <summary>当前这一次拖拽：开始、跟随、成功落地和失败回源。</summary>
    internal sealed class CurrentDrag {
        readonly DragController _controller;
        readonly DragRegistry _registry;
        readonly DragHitTester _hitTester;
        readonly DragVisual _visual = new DragVisual();

        DragSource _dragging;
        DragPlace _hover;
        PointerEventData _lastPointer;
        bool _returning;
        bool _stopping;
        int _currentDragId;

        // 记下控制器和登记表，并准备命中检测。
        public CurrentDrag(DragController controller, DragRegistry registry) {
            _controller = controller;
            _registry = registry;
            _hitTester = new DragHitTester(registry, controller);
        }

        // 开始拖拽。回源未完成时拒绝；已有别的源则先立刻还原。
        public void Begin(DragSource source, PointerEventData eventData) {
            if (CanBegin(source) == false) {
                return;
            }

            if (_dragging == source) {
                return;
            }

            if (_dragging != null) {
                ForceStop();
            }

            _currentDragId++;
            _dragging = source;
            _returning = false;
            _lastPointer = eventData;
            source.SetDragLocked(true);
            _visual.Begin(source, eventData, _controller.EffectiveSettings, _controller.ResolveDragLayer());
            source.RaiseBeginDrag(eventData);
        }

        // 跟随指针，并在进入/离开放置点时派发回调。
        public void Drag(DragSource source, PointerEventData eventData) {
            if (_dragging != source || _returning == true) {
                return;
            }

            _lastPointer = eventData;
            _visual.Follow(source, eventData);
            source.RaiseDragging(eventData);

            DragPlace next = _hitTester.Find(source, eventData, false);
            if (next == _hover) {
                return;
            }

            DragPlace previous = _hover;
            _hover = null;
            if (previous != null) {
                DragEventArgs exitArgs = CreateArgs(source, previous, eventData);
                previous.RaiseExited(exitArgs);
                _controller.RaisePlaceExited(exitArgs);
            }

            if (_dragging != source) {
                return;
            }

            if (next != null && _registry.ContainsPlace(next) == false) {
                next = null;
            }

            _hover = next;
            if (_hover == null) {
                return;
            }

            DragEventArgs enterArgs = CreateArgs(source, _hover, eventData);
            _hover.RaiseEntered(enterArgs);
            _controller.RaisePlaceEntered(enterArgs);
        }

        // 结束拖拽：能放下则成功，否则派发失败并回源。
        public void End(DragSource source, PointerEventData eventData) {
            if (_dragging != source || _returning == true) {
                return;
            }

            _lastPointer = eventData;
            source.RaiseEndDrag(eventData);
            if (_dragging != source) {
                return;
            }

            DragPlace place = _hitTester.Find(source, eventData, true);
            DragEventArgs args = CreateArgs(source, place, eventData);
            if (place != null && _controller.EvaluateCanPlace(source, place) == true) {
                // 同组且能接受，按成功落地。
                Succeed(source, place, args);
            }
            else {
                // 放不下去就派发失败并回源。
                Fail(source, place, eventData, args);
            }
        }

        // 放置点被移出时，若它正被悬停则派发离开。
        public void NotifyPlaceRemoved(DragPlace place) {
            if (_hover != place || place == null) {
                return;
            }

            DragEventArgs exitArgs = CreateArgs(_dragging, place, _lastPointer);
            _hover = null;
            place.RaiseExited(exitArgs);
            _controller.RaisePlaceExited(exitArgs);
        }

        // 源被移出时立刻还原它的视觉并清掉当前拖拽。
        public void NotifySourceRemoved(DragSource source) {
            if (_dragging != source) {
                return;
            }

            ForceStop();
        }

        // 立刻结束当前拖拽或回源，视觉回到原点。
        public void ForceStop() {
            if (_stopping == true) {
                return;
            }

            if (_dragging == null && _returning == false) {
                return;
            }

            _stopping = true;
            _currentDragId++;
            try {
                DragSource source = _dragging;
                _visual.RestoreImmediate(source);
                ClearDrag();
            }
            finally {
                _stopping = false;
            }
        }

        // 回源未完成、源被关掉或不在登记表里时不能开始。
        bool CanBegin(DragSource source) {
            if (_controller.AllowsDrag() == false) {
                return false;
            }

            if (_returning == true) {
                return false;
            }

            if (source == null || source.EnableDrag == false || source.isActiveAndEnabled == false) {
                return false;
            }

            if (source.DragTarget == null) {
                return false;
            }

            if (_registry.ContainsSource(source) == false) {
                return false;
            }

            return true;
        }

        // 成功落地后清掉当前拖拽，再派发成功。
        void Succeed(DragSource source, DragPlace place, DragEventArgs args) {
            _currentDragId++;
            _visual.ApplySuccess(source, place, _controller.EffectiveSettings);
            ClearDrag();
            place.RaiseSucceeded(args);
            source.RaiseSucceeded(args);
            _controller.RaiseSucceeded(args);
            source.RebindIfPlaying();
        }

        // 派发失败。回调没有打断时再播回源。
        void Fail(DragSource source, DragPlace place, PointerEventData eventData, DragEventArgs args) {
            DragPlace rejected = place != null ? place : _hover;
            if (_hover != null && _hover != place) {
                DragEventArgs exitArgs = CreateArgs(source, _hover, eventData);
                DragPlace hover = _hover;
                _hover = null;
                hover.RaiseExited(exitArgs);
                _controller.RaisePlaceExited(exitArgs);
            }

            if (_dragging != source) {
                return;
            }

            int currentDragId = _currentDragId;
            _returning = true;
            source.RaiseFailed(args);
            _controller.RaiseFailed(args);
            if (rejected != null) {
                DragEventArgs placeArgs = CreateArgs(source, rejected, eventData);
                rejected.RaiseFailed(placeArgs);
            }

            if (currentDragId != _currentDragId || _dragging != source) {
                return;
            }

            RunReturn(source, _visual.Generation, currentDragId).Forget();
        }

        // 回源动画。拖拽编号变了说明已经开始下一次拖拽或被 StopDrag 打断。
        async UniTask RunReturn(DragSource source, int generation, int currentDragId) {
            await _visual.PlayReturn(source, _controller.EffectiveSettings, generation);

            if (generation != _visual.Generation || currentDragId != _currentDragId || _dragging != source) {
                return;
            }

            ClearDrag();
            if (source != null) {
                source.RebindIfPlaying();
            }
        }

        // 清掉当前拖拽，并放开源上的改父锁。
        void ClearDrag() {
            DragSource source = _dragging;
            _dragging = null;
            _hover = null;
            _lastPointer = null;
            _returning = false;
            if (source != null) {
                source.SetDragLocked(false);
            }
        }

        // 组装回调载荷。组优先用登记表里的值。
        DragEventArgs CreateArgs(DragSource source, DragPlace place, PointerEventData pointer) {
            string groupId = string.Empty;
            if (source != null && _registry.TryGetSourceGroup(source, out string registered) == true) {
                // 用登记时记下的组。
                groupId = registered;
            }
            else if (source != null) {
                // 还没登记时用源上当前的组。
                groupId = source.GroupId;
            }

            return new DragEventArgs(source, place, pointer, groupId);
        }
    }
}
