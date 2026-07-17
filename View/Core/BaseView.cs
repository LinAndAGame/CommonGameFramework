using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CommonGameFramework.View
{
    /// <summary>
    /// 视图基类：由 ViewFlowController 驱动 Open/Hide 生命周期。
    /// 游戏层负责挂到 UICanvasRoot.GetLayerRoot；勿在 Awake 做初始化。
    /// </summary>
    public abstract class BaseView : MonoBehaviour
    {
        public virtual ViewLayer DefaultLayer => DefaultViewLayer.Instance;
        public ViewLayer Layer { get; set; } = DefaultViewLayer.Instance;
        public bool IsVisible { get; private set; }
        public bool IsCreated { get; private set; }

        /// <summary>首次 Open 时调用一次（同步）。</summary>
        public virtual void OnCreate() { }

        /// <summary>首次 Open 时调用一次（异步）。</summary>
        public virtual UniTask OnCreateAsync() => UniTask.CompletedTask;

        /// <summary>每次显示时调用（含从下层恢复）。</summary>
        public virtual void OnDisplay(object param) { }

        /// <summary>每次显示时调用（异步）。</summary>
        public virtual UniTask OnDisplayAsync(object param) => UniTask.CompletedTask;

        /// <summary>隐藏时调用（同步）。</summary>
        public virtual void OnHide() { }

        /// <summary>隐藏时调用（异步）。</summary>
        public virtual UniTask OnHideAsync() => UniTask.CompletedTask;

        internal void MarkCreated() => IsCreated = true;

        internal void SetVisible(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);
        }
    }
}
