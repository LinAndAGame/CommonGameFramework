using System;
using System.Collections.Generic;
using System.Threading;
using CommonGameFramework.Command;
using Cysharp.Threading.Tasks;

namespace CommonGameFramework.Presentation
{
    /// <summary>
    /// 主序列调度器：Logic / Presentation 编排入口；Busy 时游戏层应锁定输入。
    /// 取消（Cancel 或 token）会清空未执行队列，不会从断点续跑。
    /// </summary>
    public sealed class ActionScheduler
    {
        readonly ActionBus _bus = new ActionBus();
        readonly Queue<IScheduleStep> _mainQueue = new Queue<IScheduleStep>();

        CancellationTokenSource _runCts;

        public bool IsBusy { get; private set; }

        public ActionScheduler Clear()
        {
            _mainQueue.Clear();
            _bus.Clear();
            return this;
        }

        public void Cancel()
        {
            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = null;
            Clear();
            IsBusy = false;
        }

        public ActionScheduler EnqueueLogic(IGameCommand command)
        {
            if (command == null) return this;
            _mainQueue.Enqueue(new LogicStep(command));
            return this;
        }

        public ActionScheduler EnqueuePresentation(PresentationChannel channel, IPresentationAction action)
        {
            if (channel == null || action == null) return this;
            _mainQueue.Enqueue(new PresentationStep(channel, action));
            return this;
        }

        public ActionScheduler EnqueueParallel(params (PresentationChannel Channel, IPresentationAction Action)[] actions)
        {
            if (actions == null || actions.Length == 0) return this;
            _mainQueue.Enqueue(new ParallelStep(actions));
            return this;
        }

        public ActionScheduler EnqueueStep(IScheduleStep step)
        {
            if (step != null) _mainQueue.Enqueue(step);
            return this;
        }

        /// <summary>嵌套子序列，内部转为 SerialGroupStep 入队。</summary>
        public ActionScheduler EnqueueGroup(Action<ActionScheduler> buildNested)
        {
            if (buildNested == null) return this;

            var nested = new ActionScheduler();
            buildNested(nested);
            return EnqueueStep(nested.DrainStepsAsSerialGroup());
        }

        public async UniTask RunAsync(PresentationContext context)
        {
            if (context == null) return;

            _runCts?.Cancel();
            _runCts?.Dispose();
            _runCts = new CancellationTokenSource();

            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _runCts.Token,
                context.CancellationToken);

            var runContext = new PresentationContext(context.CommandContext, linkedCts.Token);

            IsBusy = true;
            try
            {
                while (_mainQueue.Count > 0)
                {
                    runContext.CancellationToken.ThrowIfCancellationRequested();
                    var step = _mainQueue.Dequeue();
                    await step.ExecuteAsync(runContext, _bus);
                }
            }
            catch (OperationCanceledException)
            {
                // 与 Cancel() 一致：取消即丢弃剩余主序列，不从断点续跑
                Clear();
                throw;
            }
            finally
            {
                linkedCts.Dispose();
                _runCts?.Dispose();
                _runCts = null;
                IsBusy = false;
            }
        }

        SerialGroupStep DrainStepsAsSerialGroup()
        {
            if (_mainQueue.Count == 0) return new SerialGroupStep();

            var steps = new IScheduleStep[_mainQueue.Count];
            for (var i = 0; i < steps.Length; i++)
                steps[i] = _mainQueue.Dequeue();

            return new SerialGroupStep(steps);
        }
    }
}
