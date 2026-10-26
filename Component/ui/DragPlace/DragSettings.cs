using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CommonGameFramework.DragPlace {
    /// <summary>
    /// 拖放配置。未赋值时使用字段默认值（运行时一份隐藏实例）。
    /// 回源缓动使用 DOTween 的 Ease。框架依赖 DOTween 与 UniTask。
    /// </summary>
    [CreateAssetMenu(fileName = "DragSettings", menuName = "CommonGameFramework/Drag Settings")]
    public sealed class DragSettings : ScriptableObject {
        [FoldoutGroup("吸附")]
        [LabelText("是否吸附")]
        [SerializeField]
        bool _snapEnabled = true;

        [FoldoutGroup("吸附")]
        [LabelText("吸附范围(像素)")]
        [ShowIf(nameof(SnapEnabled))]
        [Min(0f)]
        [SerializeField]
        float _snapRange = 80f;

        [FoldoutGroup("拖拽")]
        [LabelText("拖拽时创建副本")]
        [SerializeField]
        bool _createFakeOnDrag = true;

        [FoldoutGroup("拖拽")]
        [LabelText("拖拽时提到最前")]
        [SerializeField]
        bool _bringToFront = true;

        [FoldoutGroup("拖拽")]
        [LabelText("成功后成为放置点子节点")]
        [SerializeField]
        bool _reparentToPlaceOnSuccess = true;

        [FoldoutGroup("回源")]
        [LabelText("失败回源时长(秒)")]
        [Min(0f)]
        [SerializeField]
        float _returnDuration = 0.25f;

        [FoldoutGroup("回源")]
        [LabelText("失败回源缓动")]
        [SerializeField]
        Ease _returnEase = Ease.OutQuad;

        public bool SnapEnabled => _snapEnabled;

        public float SnapRange => _snapRange;

        public bool CreateFakeOnDrag => _createFakeOnDrag;

        public bool BringToFront => _bringToFront;

        public bool ReparentToPlaceOnSuccess => _reparentToPlaceOnSuccess;

        public float ReturnDuration => _returnDuration;

        public Ease ReturnEase => _returnEase;
    }
}
