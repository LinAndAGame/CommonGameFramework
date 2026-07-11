using Cysharp.Threading.Tasks;

namespace CardMaster.Framework.View
{
    /// <summary>
    /// 视图流程：首次 Open 走 OnCreate，每次 Open 走 OnDisplay。
    /// </summary>
    public class ViewFlowController
    {
        readonly ViewStackManager _stackManager = new ViewStackManager();
        BaseView _busyView;

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
                if (top != null && top != view) await HideViewAsync(top);
                _stackManager.Replace(options.Layer, view);
            }
            else if (options.StackMode == ViewStackMode.Push)
            {
                var top = _stackManager.GetTop(options.Layer);
                if (top != null && top != view) await HideViewAsync(top);
                _stackManager.Push(options.Layer, view);
            }

            view.OnDisplay(options.Param);
            await view.OnDisplayAsync(options.Param);
            view.SetVisible(true);
        }

        public async UniTask HideAsync(BaseView view)
        {
            if (view == null) return;
            await HideViewAsync(view);
        }

        async UniTask HideViewAsync(BaseView view)
        {
            if (_busyView == view) return;
            _busyView = view;
            view.OnHide();
            await view.OnHideAsync();
            view.SetVisible(false);
            _busyView = null;
        }
    }
}
