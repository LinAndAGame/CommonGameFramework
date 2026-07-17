using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 嵌套复合：子 Step 顺序执行。
    /// </summary>
    public sealed class SerialGroupStep : IScheduleStep
    {
        readonly List<IScheduleStep> _steps = new List<IScheduleStep>();

        public SerialGroupStep(params IScheduleStep[] steps)
        {
            if (steps == null) return;
            _steps.AddRange(steps);
        }

        public async UniTask ExecuteAsync(PresentationContext context, ActionBus bus)
        {
            for (var i = 0; i < _steps.Count; i++)
                await _steps[i].ExecuteAsync(context, bus);
        }
    }
}
