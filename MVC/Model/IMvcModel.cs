using System;

namespace CommonGameFramework.MVC
{
    /// <summary>只读模型契约。快照须为不可变值或独立副本，不得暴露可修改的权威状态。</summary>
    public interface IMvcModel<TSnapshot>
    {
        event Action<TSnapshot> Changed;
        TSnapshot CaptureSnapshot();
    }
}
