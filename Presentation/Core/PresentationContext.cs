using System.Threading;
using CommonGameFramework.Command;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 表现层上下文；Presentation 专用字段放此处，不膨胀 CommandContext。
    /// </summary>
    public sealed class PresentationContext
    {
        public CommandContext CommandContext { get; }
        public CancellationToken CancellationToken { get; }

        public PresentationContext(CommandContext commandContext, CancellationToken cancellationToken = default)
        {
            CommandContext = commandContext;
            CancellationToken = cancellationToken;
        }
    }
}
