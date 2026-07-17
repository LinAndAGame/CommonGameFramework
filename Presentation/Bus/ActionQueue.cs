using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 单通道 FIFO 队列；同一实例内严格串行执行（仅 ActionBus 使用）。
    /// </summary>
    internal sealed class ActionQueue
    {
        readonly Queue<IPresentationAction> _pending = new Queue<IPresentationAction>();

        public int Count => _pending.Count;

        public void Enqueue(IPresentationAction action)
        {
            if (action == null) return;
            _pending.Enqueue(action);
        }

        public void Clear()
        {
            _pending.Clear();
        }

        public async UniTask DrainAsync(PresentationContext context)
        {
            while (_pending.Count > 0)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                var action = _pending.Dequeue();
                await action.ExecuteAsync(context);
            }
        }
    }
}
