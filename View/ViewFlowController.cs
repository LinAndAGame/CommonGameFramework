using Cysharp.Threading.Tasks;

namespace CommonGameFramework.View
{
    /// <summary>
    /// 视图流程编排入口：Open 入栈，Hide 出栈并恢复下层。日常只用 OpenAsync / HideAsync。
    /// </summary>
    public class ViewFlowController
    {
        readonly ViewStackManager _stackManager = new ViewStackManager();
        BaseView _busyView;

        /// <summary>进阶调试用；日常请走 OpenAsync / HideAsync。</summary>
        public ViewStackManager Stack => _stackManager;

        public async UniTask OpenAsync(BaseView view, ViewOpenOptions options = null)
        {
            if (view == null) return;
            options ??= new ViewOpenOptions();
            view.Layer = options.Layer;

            if (view.IsCreated == false)
            {
                view.OnCreate();
                await view.OnCreateAsync();
                view.MarkCreated();
            }

            if (options.StackMode == ViewStackMode.Replace)
            {
                var top = _stackManager.GetTop(options.Layer);
                if (top != null && top != view) await HideViewContentAsync(top);
                _stackManager.Replace(options.Layer, view);
            }
            else if (options.StackMode == ViewStackMode.Push)
            {
                var top = _stackManager.GetTop(options.Layer);
                if (top == view)
                {
                    // 已是栈顶：只刷新，不重复入栈
                }
                else
                {
                    // 已在栈中（非栈顶）：先移除，再提到栈顶
                    if (_stackManager.Contains(options.Layer, view))
                        _stackManager.Remove(options.Layer, view);

                    if (top != null) await HideViewContentAsync(top);
                    _stackManager.Push(options.Layer, view);
                }
            }

            view.OnDisplay(options.Param);
            await view.OnDisplayAsync(options.Param);
            view.SetVisible(true);
        }

        public async UniTask HideAsync(BaseView view)
        {
            if (view == null) return;

            var layer = view.Layer ?? DefaultViewLayer.Instance;
            var top = _stackManager.GetTop(layer);
            var wasTop = top == view;

            if (wasTop)
                _stackManager.Pop(layer);
            else
                _stackManager.Remove(layer, view);

            await HideViewContentAsync(view);

            if (wasTop == false) return;

            var beneath = _stackManager.GetTop(layer);
            if (beneath == null) return;

            beneath.OnDisplay(null);
            await beneath.OnDisplayAsync(null);
            beneath.SetVisible(true);
        }

        async UniTask HideViewContentAsync(BaseView view)
        {
            if (_busyView == view) return;
            _busyView = view;
            try
            {
                view.OnHide();
                await view.OnHideAsync();
                view.SetVisible(false);
            }
            finally
            {
                if (_busyView == view)
                    _busyView = null;
            }
        }
    }
}
