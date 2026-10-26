using UnityEngine.EventSystems;

namespace CommonGameFramework.DragPlace
{
    /// <summary>一次拖放回调的只读载荷。</summary>
    public sealed class DragEventArgs
    {
        public DragEventArgs(DragSource source, DragPlace place, PointerEventData pointer, string groupId)
        {
            Source = source;
            Place = place;
            Pointer = pointer;
            GroupId = groupId ?? string.Empty;
            if (source != null)
            {
                UserData = source.UserData;
            }
        }

        public DragSource Source { get; }

        public DragPlace Place { get; }

        public PointerEventData Pointer { get; }

        public string GroupId { get; }

        public object UserData { get; }
    }
}
