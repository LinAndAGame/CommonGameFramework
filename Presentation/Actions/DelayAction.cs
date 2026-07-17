using System;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 固定毫秒延迟。
    /// </summary>
    public sealed class DelayAction : IPresentationAction
    {
        readonly int _milliseconds;

        public DelayAction(int milliseconds)
        {
            _milliseconds = milliseconds;
        }

        public UniTask ExecuteAsync(PresentationContext context)
        {
            return UniTask.Delay(_milliseconds, cancellationToken: context.CancellationToken);
        }
    }
}
