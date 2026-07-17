using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 向指定通道 Post 单个表现动作并等待该通道 Drain 完成。
    /// </summary>
    public sealed class PresentationStep : IScheduleStep
    {
        readonly PresentationChannel _channel;
        readonly IPresentationAction _action;

        public PresentationStep(PresentationChannel channel, IPresentationAction action)
        {
            _channel = channel;
            _action = action;
        }

        public async UniTask ExecuteAsync(PresentationContext context, ActionBus bus)
        {
            bus.Post(_channel, _action);
            await bus.DrainAsync(_channel, context);
        }
    }
}
