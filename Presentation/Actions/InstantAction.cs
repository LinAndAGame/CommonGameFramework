using System;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 同步回调，零等待。
    /// </summary>
    public sealed class InstantAction : IPresentationAction
    {
        readonly Action<PresentationContext> _callback;

        public InstantAction(Action<PresentationContext> callback)
        {
            _callback = callback;
        }

        public UniTask ExecuteAsync(PresentationContext context)
        {
            _callback?.Invoke(context);
            return UniTask.CompletedTask;
        }
    }
}
