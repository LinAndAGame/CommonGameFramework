using System;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 委托包装的表现动作。
    /// </summary>
    public sealed class CallbackAction : IPresentationAction
    {
        readonly Func<PresentationContext, UniTask> _callback;

        public CallbackAction(Func<PresentationContext, UniTask> callback)
        {
            _callback = callback;
        }

        public UniTask ExecuteAsync(PresentationContext context)
        {
            return _callback != null ? _callback(context) : UniTask.CompletedTask;
        }
    }
}
