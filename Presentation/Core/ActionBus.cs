using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 表现队列总线：Post 写入对应通道，Drain 时才执行。
    /// </summary>
    public sealed class ActionBus
    {
        readonly Dictionary<PresentationChannel, ActionQueue> _queues = new Dictionary<PresentationChannel, ActionQueue>();

        public void Post(PresentationChannel channel, IPresentationAction action)
        {
            if (channel == null || action == null) return;
            GetOrCreateQueue(channel).Enqueue(action);
        }

        public UniTask DrainAsync(PresentationChannel channel, PresentationContext context)
        {
            if (channel == null) return UniTask.CompletedTask;
            return GetOrCreateQueue(channel).DrainAsync(context);
        }

        /// <summary>并行 Drain 多个通道；同一 Channel 只 Drain 一次（同通道内仍 FIFO 串行）。</summary>
        public async UniTask DrainParallelAsync(PresentationChannel[] channels, PresentationContext context)
        {
            if (channels == null || channels.Length == 0) return;

            var unique = new List<PresentationChannel>(channels.Length);
            for (var i = 0; i < channels.Length; i++)
            {
                var channel = channels[i];
                if (channel == null) continue;
                if (unique.Contains(channel)) continue;
                unique.Add(channel);
            }

            if (unique.Count == 0) return;

            var tasks = new UniTask[unique.Count];
            for (var i = 0; i < unique.Count; i++)
                tasks[i] = DrainAsync(unique[i], context);

            await UniTask.WhenAll(tasks);
        }

        public void Clear()
        {
            foreach (var queue in _queues.Values)
                queue.Clear();
        }

        ActionQueue GetOrCreateQueue(PresentationChannel channel)
        {
            if (_queues.TryGetValue(channel, out var queue)) return queue;
            queue = new ActionQueue();
            _queues[channel] = queue;
            return queue;
        }
    }
}
