using CommonGameFramework.Command;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 同步执行逻辑 Command，不含 await。
    /// </summary>
    public sealed class LogicStep : IScheduleStep
    {
        readonly IGameCommand _command;

        public LogicStep(IGameCommand command)
        {
            _command = command;
        }

        public UniTask ExecuteAsync(PresentationContext context, ActionBus bus)
        {
            _command.Execute(context.CommandContext);
            return UniTask.CompletedTask;
        }
    }
}
