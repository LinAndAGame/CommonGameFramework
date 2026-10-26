using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonGameFramework.DragPlace {
    /// <summary>拖拽跟随物：副本或原件、射线屏蔽、成功落点、失败回源。回源动画用 DOTween，完成用 UniTask 等待。</summary>
    internal sealed class DragVisual {
        struct GraphicRaycastState {
            public Graphic Graphic;
            public bool RaycastTarget;
        }

        readonly List<GraphicRaycastState> _hiddenGraphics = new List<GraphicRaycastState>();

        int _generation;
        bool _hasOrigin;
        Transform _originParent;
        int _originSiblingIndex;
        Vector2 _originAnchoredPos;
        Vector3 _originLocalScale;
        Quaternion _originLocalRot;
        Vector3 _dragWorldOffset;
        GameObject _fakeGo;
        RectTransform _fakeRect;
        Tween _returnTween;

        public int Generation => _generation;

        // 记录原点，按配置创建副本或把原件提到拖拽层，并算好指针偏移。
        public void Begin(DragSource source, PointerEventData eventData, DragSettings settings, RectTransform dragLayer) {
            RectTransform target = source.DragTarget;
            _hasOrigin = target != null;
            if (target != null) {
                _originParent = target.parent;
                _originSiblingIndex = target.GetSiblingIndex();
                _originAnchoredPos = target.anchoredPosition;
                _originLocalScale = target.localScale;
                _originLocalRot = target.localRotation;
            }

            if (settings.CreateFakeOnDrag == true) {
                CreateFake(source, dragLayer, settings.BringToFront);
            }
            else if (settings.BringToFront == true && dragLayer != null && target != null) {
                target.SetParent(dragLayer, true);
                target.SetAsLastSibling();
            }

            if (_fakeGo != null) {
                HideRaycasts(source.gameObject);
                if (target != null && source.gameObject != target.gameObject) {
                    HideRaycasts(target.gameObject);
                }
            }
            else if (target != null) {
                HideRaycasts(target.gameObject);
                if (source.gameObject != target.gameObject) {
                    HideRaycasts(source.gameObject);
                }
            }

            CaptureOffset(source, eventData);
        }

        // 让当前跟随物贴住指针，保留按下时的世界偏移。
        public void Follow(DragSource source, PointerEventData eventData) {
            RectTransform moving = GetMoving(source);
            if (moving == null || eventData == null) {
                return;
            }

            RectTransform parent = moving.parent as RectTransform;
            if (parent == null) {
                return;
            }

            bool converted = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPos);
            if (converted == false) {
                return;
            }

            moving.position = new Vector3(
                worldPos.x + _dragWorldOffset.x,
                worldPos.y + _dragWorldOffset.y,
                moving.position.z);
        }

        // 成功时销毁副本，或把原件换父/吸附到放置点，并恢复射线。
        public void ApplySuccess(DragSource source, DragPlace place, DragSettings settings) {
            _generation++;
            KillReturnTween();
            if (_fakeGo != null) {
                DestroyFake();
            }
            else {
                PlaceOrigin(source, place, settings);
            }

            RestoreRaycasts();
            ForgetOrigin();
        }

        // 打断回源或新拖拽前，把视觉立刻收回原点。
        public void RestoreImmediate(DragSource source) {
            _generation++;
            KillReturnTween();
            if (_fakeGo != null) {
                DestroyFake();
            }
            else {
                SnapOrigin(source, false);
            }

            RestoreRaycasts();
            ForgetOrigin();
        }

        // 失败回源。时长为 0 时瞬间完成。StopDrag 或销毁会 Kill 当前 tween。
        public async UniTask PlayReturn(DragSource source, DragSettings settings, int generation) {
            if (generation != _generation) {
                return;
            }

            KillReturnTween();
            float duration = settings.ReturnDuration;
            Ease ease = settings.ReturnEase;
            if (_fakeRect != null) {
                // 有副本时让副本飞回原件。
                await ReturnFake(source, duration, ease, generation);
            }
            else {
                // 没有副本时让原件飞回原点。
                await ReturnOrigin(source, duration, ease, generation);
            }

            if (generation != _generation) {
                return;
            }

            KillReturnTween();
            DestroyFake();
            RestoreRaycasts();
            ForgetOrigin();
        }

        // 副本飞回原件世界坐标，结束后销毁副本。
        async UniTask ReturnFake(DragSource source, float duration, Ease ease, int generation) {
            RectTransform fake = _fakeRect;
            RectTransform origin = source != null ? source.DragTarget : null;
            if (fake == null || origin == null) {
                DestroyFake();
                return;
            }

            if (duration <= 0f) {
                DestroyFake();
                return;
            }

            Sequence seq = DOTween.Sequence();
            seq.Join(fake.DOMove(origin.position, duration).SetEase(ease));
            _returnTween = seq;
            await seq.AsyncWaitForCompletion();
            if (generation != _generation) {
                return;
            }

            DestroyFake();
        }

        // 原件挂回拖拽前的父节点，再缓动局部位置、缩放和旋转。
        async UniTask ReturnOrigin(DragSource source, float duration, Ease ease, int generation) {
            if (source == null) {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null) {
                return;
            }

            if (_originParent != null) {
                target.SetParent(_originParent, true);
                target.SetSiblingIndex(_originSiblingIndex);
            }

            if (duration <= 0f) {
                SnapOrigin(source, true);
                return;
            }

            Sequence seq = DOTween.Sequence();
            seq.Join(target.DOAnchorPos(_originAnchoredPos, duration).SetEase(ease));
            seq.Join(target.DOScale(_originLocalScale, duration).SetEase(ease));
            seq.Join(target.DOLocalRotate(_originLocalRot.eulerAngles, duration).SetEase(ease));
            _returnTween = seq;
            await seq.AsyncWaitForCompletion();
        }

        // 停掉正在播放的回源 tween。
        void KillReturnTween() {
            if (_returnTween != null && _returnTween.IsActive() == true) {
                _returnTween.Kill();
            }

            _returnTween = null;
        }

        // 成功时按配置把原件换父，并吸附到放置点。
        void PlaceOrigin(DragSource source, DragPlace place, DragSettings settings) {
            if (source == null || place == null || place.RectTransform == null) {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null) {
                return;
            }

            if (settings.ReparentToPlaceOnSuccess == true) {
                // 成功后把原件挂到放置点下。
                target.SetParent(place.RectTransform, true);
            }

            if (settings.SnapEnabled == true) {
                // 成功后贴到放置点坐标。
                target.position = place.RectTransform.position;
            }
        }

        // worldPositionStays 为 false 时先挂回原父节点再写局部变换，避免闪一帧。
        void SnapOrigin(DragSource source, bool alreadyReparented) {
            if (_hasOrigin == false || source == null) {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null) {
                return;
            }

            if (alreadyReparented == false && _originParent != null) {
                target.SetParent(_originParent, false);
                target.SetSiblingIndex(_originSiblingIndex);
            }

            target.anchoredPosition = _originAnchoredPos;
            target.localScale = _originLocalScale;
            target.localRotation = _originLocalRot;
        }

        // 记下按下时指针和跟随物的世界偏移。
        void CaptureOffset(DragSource source, PointerEventData eventData) {
            _dragWorldOffset = Vector3.zero;
            if (eventData == null) {
                return;
            }

            RectTransform moving = GetMoving(source);
            if (moving == null) {
                return;
            }

            RectTransform parent = moving.parent as RectTransform;
            if (parent == null) {
                return;
            }

            bool converted = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 pressWorld);
            if (converted == false) {
                return;
            }

            _dragWorldOffset = moving.position - pressWorld;
        }

        // 有副本时跟副本，否则跟原件。
        RectTransform GetMoving(DragSource source) {
            RectTransform moving = null;
            if (_fakeRect != null) {
                // 拖的是副本。
                moving = _fakeRect;
            }
            else if (source != null) {
                // 拖的是原件。
                moving = source.DragTarget;
            }

            return moving;
        }

        // 克隆原件到拖拽层，并关掉副本上的拖放组件。
        void CreateFake(DragSource source, RectTransform dragLayer, bool bringToFront) {
            DestroyFake();
            RectTransform origin = source.DragTarget;
            if (origin == null || dragLayer == null) {
                return;
            }

            DragCloneGuard.Enter();
            GameObject fakeGo;
            try {
                fakeGo = Object.Instantiate(origin.gameObject, dragLayer, true);
            }
            finally {
                DragCloneGuard.Exit();
            }

            fakeGo.name = origin.name + "_DragFake";
            DisableFakeInteraction(fakeGo);
            _fakeGo = fakeGo;
            _fakeRect = fakeGo.transform as RectTransform;
            if (_fakeRect != null) {
                _fakeRect.position = origin.position;
                _fakeRect.rotation = origin.rotation;
                MatchWorldScale(_fakeRect, origin);
            }

            if (bringToFront == true) {
                fakeGo.transform.SetAsLastSibling();
            }
        }

        // 让副本的世界缩放和原件一致。
        void MatchWorldScale(RectTransform fake, RectTransform origin) {
            RectTransform parent = fake.parent as RectTransform;
            Vector3 parentLossy = parent != null ? parent.lossyScale : Vector3.one;
            Vector3 originLossy = origin.lossyScale;
            fake.localScale = new Vector3(
                DivideScale(originLossy.x, parentLossy.x),
                DivideScale(originLossy.y, parentLossy.y),
                DivideScale(originLossy.z, parentLossy.z));
        }

        // 父节点这一轴缩放为 0 时保留原缩放，避免除零。
        float DivideScale(float originLossy, float parentLossy) {
            float scale = originLossy;
            if (Mathf.Approximately(parentLossy, 0f) == true) {
                // 父节点这一轴没有缩放。
                scale = originLossy;
            }
            else {
                // 把世界缩放换回本地缩放。
                scale = originLossy / parentLossy;
            }

            return scale;
        }

        // 关掉副本上的射线、拖拽源、放置点和控制器。
        void DisableFakeInteraction(GameObject fakeGo) {
            CanvasGroup[] groups = fakeGo.GetComponentsInChildren<CanvasGroup>(true);
            for (int i = 0; i < groups.Length; i++) {
                groups[i].blocksRaycasts = false;
                groups[i].interactable = false;
            }

            Graphic[] graphics = fakeGo.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++) {
                graphics[i].raycastTarget = false;
            }

            DragSource[] sources = fakeGo.GetComponentsInChildren<DragSource>(true);
            for (int i = 0; i < sources.Length; i++) {
                sources[i].MarkAsClone();
                sources[i].enabled = false;
            }

            DragPlace[] places = fakeGo.GetComponentsInChildren<DragPlace>(true);
            for (int i = 0; i < places.Length; i++) {
                places[i].MarkAsClone();
                places[i].enabled = false;
            }

            DragPointerBridge[] bridges = fakeGo.GetComponentsInChildren<DragPointerBridge>(true);
            for (int i = 0; i < bridges.Length; i++) {
                bridges[i].enabled = false;
            }

            DragController[] controllers = fakeGo.GetComponentsInChildren<DragController>(true);
            for (int i = 0; i < controllers.Length; i++) {
                controllers[i].MarkAsClone();
                controllers[i].enabled = false;
            }
        }

        // 拖拽期间关掉射线，避免副本挡住放置点。
        void HideRaycasts(GameObject root) {
            if (root == null) {
                return;
            }

            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++) {
                Graphic graphic = graphics[i];
                if (graphic == null || AlreadyHidden(graphic) == true) {
                    continue;
                }

                _hiddenGraphics.Add(new GraphicRaycastState {
                    Graphic = graphic,
                    RaycastTarget = graphic.raycastTarget
                });
                graphic.raycastTarget = false;
            }
        }

        // 同一张图不要重复记下原来的 raycastTarget。
        bool AlreadyHidden(Graphic graphic) {
            for (int i = 0; i < _hiddenGraphics.Count; i++) {
                if (_hiddenGraphics[i].Graphic == graphic) {
                    return true;
                }
            }

            return false;
        }

        // 按拖拽前的值恢复射线。
        void RestoreRaycasts() {
            for (int i = 0; i < _hiddenGraphics.Count; i++) {
                GraphicRaycastState state = _hiddenGraphics[i];
                if (state.Graphic != null) {
                    state.Graphic.raycastTarget = state.RaycastTarget;
                }
            }

            _hiddenGraphics.Clear();
        }

        // 销毁跟随用的副本。
        void DestroyFake() {
            if (_fakeGo != null) {
                Object.Destroy(_fakeGo);
            }

            _fakeGo = null;
            _fakeRect = null;
        }

        // 丢掉原点记录，避免下一次拖拽用到旧父节点。
        void ForgetOrigin() {
            _hasOrigin = false;
            _originParent = null;
        }
    }
}
