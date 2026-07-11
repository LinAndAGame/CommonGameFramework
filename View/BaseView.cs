using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CardMaster.Framework.View
{
    public abstract class BaseView : MonoBehaviour
    {
        public virtual UILayer DefaultLayer => UILayer.Main;
        public UILayer Layer { get; set; } = UILayer.Main;
        public bool IsVisible { get; private set; }
        public bool IsCreated { get; private set; }

        public virtual void OnCreate() { }
        public virtual UniTask OnCreateAsync() => UniTask.CompletedTask;
        public virtual void OnDisplay(object param) { }
        public virtual UniTask OnDisplayAsync(object param) => UniTask.CompletedTask;
        public virtual void OnHide() { }
        public virtual UniTask OnHideAsync() => UniTask.CompletedTask;

        internal void MarkCreated() => IsCreated = true;

        internal void SetVisible(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);
        }
    }
}
