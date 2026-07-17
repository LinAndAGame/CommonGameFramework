using UnityEngine;

namespace CommonGameFramework.Extensions
{
    /// <summary>RectTransform 扩展。</summary>
    public static class RectTransformExtensions
    {
        public static void SetAnchoredPositionX(this RectTransform rect, float x)
        {
            if (rect == null) return;
            var p = rect.anchoredPosition;
            p.x = x;
            rect.anchoredPosition = p;
        }

        public static void SetAnchoredPositionY(this RectTransform rect, float y)
        {
            if (rect == null) return;
            var p = rect.anchoredPosition;
            p.y = y;
            rect.anchoredPosition = p;
        }

        public static void SetSizeDeltaX(this RectTransform rect, float width)
        {
            if (rect == null) return;
            var s = rect.sizeDelta;
            s.x = width;
            rect.sizeDelta = s;
        }

        public static void SetSizeDeltaY(this RectTransform rect, float height)
        {
            if (rect == null) return;
            var s = rect.sizeDelta;
            s.y = height;
            rect.sizeDelta = s;
        }
    }
}
