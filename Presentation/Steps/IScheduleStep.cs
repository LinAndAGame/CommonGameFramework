using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 主序列调度单元；由 ActionScheduler 串行驱动。
    /// </summary>
    public interface IScheduleStep
    {
        UniTask ExecuteAsync(PresentationContext context, ActionBus bus);
    }
}
