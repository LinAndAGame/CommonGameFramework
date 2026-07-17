using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 表现层动作：异步播动效、延迟、音效等；Instant 则返回 CompletedTask。
    /// </summary>
    public interface IPresentationAction
    {
        UniTask ExecuteAsync(PresentationContext context);
    }
}
