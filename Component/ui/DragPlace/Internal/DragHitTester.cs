using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CommonGameFramework.DragPlace {
    /// <summary>用射线、矩形和吸附范围寻找同组放置点。</summary>
    internal sealed class DragHitTester {
        readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
        readonly DragRegistry _registry;
        readonly DragController _controller;

        // 记下登记表和控制器，命中时用它们过滤同组放置点。
        public DragHitTester(DragRegistry registry, DragController controller) {
            _registry = registry;
            _controller = controller;
        }

        // 找指针下的同组放置点。requireAccept 为真时还要能通过放置判定。
        public DragPlace Find(DragSource source, PointerEventData eventData, bool requireAccept) {
            DragPlace found = null;
            if (source == null || eventData == null) {
                // 没有源或指针。
                found = null;
            }
            else if (EventSystem.current == null) {
                // 没有事件系统时改用矩形和吸附范围。
                found = FindFallback(source, eventData, requireAccept);
            }
            else {
                // 射线从前往后找第一个同组放置点。
                _raycastResults.Clear();
                EventSystem.current.RaycastAll(eventData, _raycastResults);
                for (int i = 0; i < _raycastResults.Count; i++) {
                    GameObject go = _raycastResults[i].gameObject;
                    if (go == null) {
                        continue;
                    }

                    DragPlace place = go.GetComponentInParent<DragPlace>();
                    if (place != null && IsCandidate(source, place, requireAccept) == true) {
                        found = place;
                        break;
                    }
                }

                if (found == null) {
                    // 射线没有打中可放置的点。
                    found = FindFallback(source, eventData, requireAccept);
                }
            }

            return found;
        }

        // 射线未命中时先看矩形，再看吸附范围。
        DragPlace FindFallback(DragSource source, PointerEventData eventData, bool requireAccept) {
            DragPlace found = FindByRect(source, eventData, requireAccept);
            if (found == null) {
                // 矩形没命中，再按吸附范围找。
                found = FindBySnapRange(source, eventData, requireAccept);
            }

            return found;
        }

        // 在同组放置点里取包含指针、层级最深的一个。
        DragPlace FindByRect(DragSource source, PointerEventData eventData, bool requireAccept) {
            if (_registry.TryGetSourceGroup(source, out string groupId) == false) {
                return null;
            }

            IReadOnlyList<DragPlace> places = _registry.GetPlaces(groupId);
            DragPlace matched = null;
            int matchedDepth = -1;
            for (int i = 0; i < places.Count; i++) {
                DragPlace place = places[i];
                if (IsCandidate(source, place, requireAccept) == false) {
                    continue;
                }

                if (place.ContainsScreenPoint(eventData.position, eventData.pressEventCamera) == false) {
                    continue;
                }

                int depth = GetHierarchyDepth(place.transform);
                if (depth >= matchedDepth) {
                    matched = place;
                    matchedDepth = depth;
                }
            }

            return matched;
        }

        // 在吸附范围内取离指针最近的同组放置点。
        DragPlace FindBySnapRange(DragSource source, PointerEventData eventData, bool requireAccept) {
            DragSettings settings = _controller.EffectiveSettings;
            if (settings.SnapEnabled == false || settings.SnapRange <= 0f) {
                return null;
            }

            if (_registry.TryGetSourceGroup(source, out string groupId) == false) {
                return null;
            }

            IReadOnlyList<DragPlace> places = _registry.GetPlaces(groupId);
            DragPlace matched = null;
            float bestDist = settings.SnapRange;
            int matchedDepth = -1;
            for (int i = 0; i < places.Count; i++) {
                DragPlace place = places[i];
                if (IsCandidate(source, place, requireAccept) == false) {
                    continue;
                }

                float dist = GetPointerDistance(place, eventData);
                if (dist > settings.SnapRange) {
                    continue;
                }

                int depth = GetHierarchyDepth(place.transform);
                bool closer = dist < bestDist;
                bool sameDistDeeper = Mathf.Approximately(dist, bestDist) == true && depth >= matchedDepth;
                if (matched == null || closer == true || sameDistDeeper == true) {
                    matched = place;
                    bestDist = dist;
                    matchedDepth = depth;
                }
            }

            return matched;
        }

        // 同组、已启用，且在需要判定时能被接受。
        bool IsCandidate(DragSource source, DragPlace place, bool requireAccept) {
            if (place == null || place.isActiveAndEnabled == false) {
                return false;
            }

            if (_registry.ContainsPlace(place) == false) {
                return false;
            }

            if (_registry.TryGetSourceGroup(source, out string sourceGroup) == false) {
                return false;
            }

            if (_registry.TryGetPlaceGroup(place, out string placeGroup) == false) {
                return false;
            }

            if (string.Equals(sourceGroup, placeGroup, System.StringComparison.Ordinal) == false) {
                return false;
            }

            if (requireAccept == true && _controller.EvaluateCanPlace(source, place) == false) {
                return false;
            }

            return true;
        }

        // 指针到放置点矩形边缘的距离，落在矩形内为 0。
        float GetPointerDistance(DragPlace place, PointerEventData eventData) {
            float distance = float.MaxValue;
            bool inside = place.ContainsScreenPoint(eventData.position, eventData.pressEventCamera);
            bool converted = false;
            Vector2 local = Vector2.zero;
            if (inside == false && place.RectTransform != null) {
                converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    place.RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out local);
            }

            if (inside == true) {
                // 指针在矩形内，距离为 0。
                distance = 0f;
            }
            else if (place.RectTransform == null || converted == false) {
                // 无法换算到本地坐标。
                distance = float.MaxValue;
            }
            else {
                // 算指针到矩形边缘的距离。
                Rect rect = place.RectTransform.rect;
                float dx = 0f;
                if (local.x < rect.xMin) {
                    // 指针在矩形左边。
                    dx = rect.xMin - local.x;
                }
                else if (local.x > rect.xMax) {
                    // 指针在矩形右边。
                    dx = local.x - rect.xMax;
                }

                float dy = 0f;
                if (local.y < rect.yMin) {
                    // 指针在矩形下边。
                    dy = rect.yMin - local.y;
                }
                else if (local.y > rect.yMax) {
                    // 指针在矩形上边。
                    dy = local.y - rect.yMax;
                }

                distance = Mathf.Sqrt((dx * dx) + (dy * dy));
            }

            return distance;
        }

        // 根节点深度为 1，子节点更大。
        int GetHierarchyDepth(Transform target) {
            int depth = 0;
            Transform current = target;
            while (current != null) {
                depth++;
                current = current.parent;
            }

            return depth;
        }
    }
}
