using UnityEngine;

namespace CommonGameFramework.Extensions
{
    /// <summary>Transform 扩展。</summary>
    public static class TransformExtensions
    {
        public static void ResetLocal(this Transform transform)
        {
            if (transform == null) return;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            if (transform == null) return;
            var p = transform.localPosition;
            p.x = x;
            transform.localPosition = p;
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            if (transform == null) return;
            var p = transform.localPosition;
            p.y = y;
            transform.localPosition = p;
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            if (transform == null) return;
            var p = transform.localPosition;
            p.z = z;
            transform.localPosition = p;
        }

        public static void SetPositionX(this Transform transform, float x)
        {
            if (transform == null) return;
            var p = transform.position;
            p.x = x;
            transform.position = p;
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            if (transform == null) return;
            var p = transform.position;
            p.y = y;
            transform.position = p;
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            if (transform == null) return;
            var p = transform.position;
            p.z = z;
            transform.position = p;
        }
    }
}
