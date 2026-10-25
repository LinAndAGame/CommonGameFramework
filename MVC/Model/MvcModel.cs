using System;

namespace CommonGameFramework.MVC
{
    /// <summary>模型基类。派生类同步结算全部状态后，再调用 PublishChanged。</summary>
    public abstract class MvcModel<TSnapshot> : IMvcModel<TSnapshot>
    {
        public event Action<TSnapshot> Changed;

        public abstract TSnapshot CaptureSnapshot();

        protected void PublishChanged()
        {
            Changed?.Invoke(CaptureSnapshot());
        }
    }
}
