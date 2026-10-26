using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonGameFramework.DragPlace
{
    /// <summary>拖拽跟随物：副本或原件、射线屏蔽、成功落点、失败回源。</summary>
    internal sealed class DragVisual
    {
        struct GraphicRaycastState
        {
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

        public int Generation => _generation;

        // 记录原点，按配置创建副本或把原件提到拖拽层，并算好指针偏移。
        public void Begin(DragSource source, PointerEventData eventData, DragSettings settings, RectTransform dragLayer)
        {
            RectTransform target = source.DragTarget;
            _hasOrigin = target != null;
            if (target != null)
            {
                _originParent = target.parent;
                _originSiblingIndex = target.GetSiblingIndex();
                _originAnchoredPos = target.anchoredPosition;
                _originLocalScale = target.localScale;
                _originLocalRot = target.localRotation;
            }

            if (settings.CreateFakeOnDrag == true)
            {
                CreateFake(source, dragLayer, settings.BringToFront);
            }
            else if (settings.BringToFront == true && dragLayer != null && target != null)
            {
                target.SetParent(dragLayer, true);
                target.SetAsLastSibling();
            }

            if (_fakeGo != null)
            {
                HideRaycasts(source.gameObject);
                if (target != null && source.gameObject != target.gameObject)
                {
                    HideRaycasts(target.gameObject);
                }
            }
            else if (target != null)
            {
                HideRaycasts(target.gameObject);
                if (source.gameObject != target.gameObject)
                {
                    HideRaycasts(source.gameObject);
                }
            }

            CaptureOffset(source, eventData);
        }

        // 让当前跟随物贴住指针，保留按下时的世界偏移。
        public void Follow(DragSource source, PointerEventData eventData)
        {
            RectTransform moving = GetMoving(source);
            if (moving == null || eventData == null)
            {
                return;
            }

            RectTransform parent = moving.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            bool converted = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPos);
            if (converted == false)
            {
                return;
            }

            moving.position = new Vector3(
                worldPos.x + _dragWorldOffset.x,
                worldPos.y + _dragWorldOffset.y,
                moving.position.z);
        }

        // 成功时销毁副本，或把原件换父/吸附到放置点，并恢复射线。
        public void ApplySuccess(DragSource source, DragPlace place, DragSettings settings)
        {
            _generation++;
            if (_fakeGo != null)
            {
                DestroyFake();
            }
            else
            {
                PlaceOrigin(source, place, settings);
            }

            RestoreRaycasts();
            ForgetOrigin();
        }

        // 打断回源或新拖拽前，把视觉立刻收回原点。
        public void RestoreImmediate(DragSource source)
        {
            _generation++;
            if (_fakeGo != null)
            {
                DestroyFake();
            }
            else
            {
                SnapOrigin(source, false);
            }

            RestoreRaycasts();
            ForgetOrigin();
        }

        // 失败回源。时长为 0 时瞬间完成。generation 变化表示被打断。
        public async UniTask PlayReturn(DragSource source, DragSettings settings, int generation, CancellationToken token)
        {
            if (generation != _generation)
            {
                return;
            }

            float duration = settings.ReturnDuration;
            DragEase ease = settings.ReturnEase;
            if (_fakeRect != null)
            {
                await ReturnFake(source, duration, ease, generation, token);
            }
            else
            {
                await ReturnOrigin(source, duration, ease, generation, token);
            }

            if (generation != _generation)
            {
                return;
            }

            DestroyFake();
            RestoreRaycasts();
            ForgetOrigin();
        }

        async UniTask ReturnFake(DragSource source, float duration, DragEase ease, int generation, CancellationToken token)
        {
            RectTransform fake = _fakeRect;
            RectTransform origin = source != null ? source.DragTarget : null;
            if (fake == null || origin == null)
            {
                DestroyFake();
                return;
            }

            Vector3 from = fake.position;
            Vector3 to = origin.position;
            if (duration <= 0f)
            {
                DestroyFake();
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (generation != _generation || fake == null)
                {
                    return;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (t > 1f)
                {
                    t = 1f;
                }

                float eased = DragEasing.Evaluate(ease, t);
                fake.position = Vector3.LerpUnclamped(from, to, eased);
                if (t >= 1f)
                {
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        async UniTask ReturnOrigin(DragSource source, float duration, DragEase ease, int generation, CancellationToken token)
        {
            if (source == null)
            {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null)
            {
                return;
            }

            if (_originParent != null)
            {
                target.SetParent(_originParent, true);
                target.SetSiblingIndex(_originSiblingIndex);
            }

            if (duration <= 0f)
            {
                SnapOrigin(source, true);
                return;
            }

            Vector2 fromPos = target.anchoredPosition;
            Vector3 fromScale = target.localScale;
            Quaternion fromRot = target.localRotation;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (generation != _generation || target == null)
                {
                    return;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                if (t > 1f)
                {
                    t = 1f;
                }

                float eased = DragEasing.Evaluate(ease, t);
                target.anchoredPosition = Vector2.LerpUnclamped(fromPos, _originAnchoredPos, eased);
                target.localScale = Vector3.LerpUnclamped(fromScale, _originLocalScale, eased);
                target.localRotation = Quaternion.SlerpUnclamped(fromRot, _originLocalRot, eased);
                if (t >= 1f)
                {
                    return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        void PlaceOrigin(DragSource source, DragPlace place, DragSettings settings)
        {
            if (source == null || place == null || place.RectTransform == null)
            {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null)
            {
                return;
            }

            if (settings.ReparentToPlaceOnSuccess == true)
            {
                target.SetParent(place.RectTransform, true);
            }

            if (settings.SnapEnabled == true)
            {
                target.position = place.RectTransform.position;
            }
        }

        // worldPositionStays 为 false 时先挂回原父节点再写局部变换，避免闪一帧。
        void SnapOrigin(DragSource source, bool alreadyReparented)
        {
            if (_hasOrigin == false || source == null)
            {
                return;
            }

            RectTransform target = source.DragTarget;
            if (target == null)
            {
                return;
            }

            if (alreadyReparented == false && _originParent != null)
            {
                target.SetParent(_originParent, false);
                target.SetSiblingIndex(_originSiblingIndex);
            }

            target.anchoredPosition = _originAnchoredPos;
            target.localScale = _originLocalScale;
            target.localRotation = _originLocalRot;
        }

        void CaptureOffset(DragSource source, PointerEventData eventData)
        {
            _dragWorldOffset = Vector3.zero;
            if (eventData == null)
            {
                return;
            }

            RectTransform moving = GetMoving(source);
            if (moving == null)
            {
                return;
            }

            RectTransform parent = moving.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            bool converted = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                parent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 pressWorld);
            if (converted == false)
            {
                return;
            }

            _dragWorldOffset = moving.position - pressWorld;
        }

        RectTransform GetMoving(DragSource source)
        {
            if (_fakeRect != null)
            {
                return _fakeRect;
            }

            if (source == null)
            {
                return null;
            }

            return source.DragTarget;
        }

        void CreateFake(DragSource source, RectTransform dragLayer, bool bringToFront)
        {
            DestroyFake();
            RectTransform origin = source.DragTarget;
            if (origin == null || dragLayer == null)
            {
                return;
            }

            DragCloneGuard.Enter();
            GameObject fakeGo;
            try
            {
                fakeGo = Object.Instantiate(origin.gameObject, dragLayer, true);
            }
            finally
            {
                DragCloneGuard.Exit();
            }

            fakeGo.name = origin.name + "_DragFake";
            DisableFakeInteraction(fakeGo);
            _fakeGo = fakeGo;
            _fakeRect = fakeGo.transform as RectTransform;
            if (_fakeRect != null)
            {
                _fakeRect.position = origin.position;
                _fakeRect.rotation = origin.rotation;
                MatchWorldScale(_fakeRect, origin);
            }

            if (bringToFront == true)
            {
                fakeGo.transform.SetAsLastSibling();
            }
        }

        void MatchWorldScale(RectTransform fake, RectTransform origin)
        {
            RectTransform parent = fake.parent as RectTransform;
            Vector3 parentLossy = parent != null ? parent.lossyScale : Vector3.one;
            Vector3 originLossy = origin.lossyScale;
            fake.localScale = new Vector3(
                DivideScale(originLossy.x, parentLossy.x),
                DivideScale(originLossy.y, parentLossy.y),
                DivideScale(originLossy.z, parentLossy.z));
        }

        static float DivideScale(float originLossy, float parentLossy)
        {
            if (Mathf.Approximately(parentLossy, 0f) == true)
            {
                return originLossy;
            }

            return originLossy / parentLossy;
        }

        static void DisableFakeInteraction(GameObject fakeGo)
        {
            CanvasGroup[] groups = fakeGo.GetComponentsInChildren<CanvasGroup>(true);
            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].blocksRaycasts = false;
                groups[i].interactable = false;
            }

            Graphic[] graphics = fakeGo.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].raycastTarget = false;
            }

            DragSource[] sources = fakeGo.GetComponentsInChildren<DragSource>(true);
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].MarkAsClone();
                sources[i].enabled = false;
            }

            DragPlace[] places = fakeGo.GetComponentsInChildren<DragPlace>(true);
            for (int i = 0; i < places.Length; i++)
            {
                places[i].MarkAsClone();
                places[i].enabled = false;
            }

            DragPointerBridge[] bridges = fakeGo.GetComponentsInChildren<DragPointerBridge>(true);
            for (int i = 0; i < bridges.Length; i++)
            {
                bridges[i].enabled = false;
            }

            DragController[] controllers = fakeGo.GetComponentsInChildren<DragController>(true);
            for (int i = 0; i < controllers.Length; i++)
            {
                controllers[i].MarkAsClone();
                controllers[i].enabled = false;
            }
        }

        void HideRaycasts(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || AlreadyHidden(graphic) == true)
                {
                    continue;
                }

                _hiddenGraphics.Add(new GraphicRaycastState
                {
                    Graphic = graphic,
                    RaycastTarget = graphic.raycastTarget
                });
                graphic.raycastTarget = false;
            }
        }

        bool AlreadyHidden(Graphic graphic)
        {
            for (int i = 0; i < _hiddenGraphics.Count; i++)
            {
                if (_hiddenGraphics[i].Graphic == graphic)
                {
                    return true;
                }
            }

            return false;
        }

        void RestoreRaycasts()
        {
            for (int i = 0; i < _hiddenGraphics.Count; i++)
            {
                GraphicRaycastState state = _hiddenGraphics[i];
                if (state.Graphic != null)
                {
                    state.Graphic.raycastTarget = state.RaycastTarget;
                }
            }

            _hiddenGraphics.Clear();
        }

        void DestroyFake()
        {
            if (_fakeGo != null)
            {
                Object.Destroy(_fakeGo);
            }

            _fakeGo = null;
            _fakeRect = null;
        }

        void ForgetOrigin()
        {
            _hasOrigin = false;
            _originParent = null;
        }
    }
}
