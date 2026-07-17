using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 向多个通道 Post 表现动作并并行 Drain；同 Channel 只 Drain 一次（该通道内动作仍串行）。
    /// </summary>
    public sealed class ParallelStep : IScheduleStep
    {
        readonly List<(PresentationChannel Channel, IPresentationAction Action)> _entries =
            new List<(PresentationChannel, IPresentationAction)>();

        public ParallelStep(params (PresentationChannel Channel, IPresentationAction Action)[] entries)
        {
            if (entries == null) return;
            _entries.AddRange(entries);
        }

        public async UniTask ExecuteAsync(PresentationContext context, ActionBus bus)
        {
            if (_entries.Count == 0) return;

            var channels = new PresentationChannel[_entries.Count];
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                bus.Post(entry.Channel, entry.Action);
                channels[i] = entry.Channel;
            }

            await bus.DrainParallelAsync(channels, context);
        }
    }
}
