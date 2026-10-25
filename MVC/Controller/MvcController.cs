using System;

namespace CommonGameFramework.MVC
{
    /// <summary>连接模型快照与视图。交互事件在 Init/OnDispose 内成对订阅，逻辑保持同步。</summary>
    public abstract class MvcController<TModel, TSnapshot, TView> : IMvcController
        where TModel : class, IMvcModel<TSnapshot>
        where TView : class, IMvcView<TSnapshot>
    {
        bool _disposed;

        protected TModel Model { get; }
        protected TView View { get; }
        public bool IsInitialized { get; private set; }

        protected MvcController(TModel model, TView view)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            View = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Initialize()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            if (IsInitialized) return;

            IsInitialized = true;
            Model.Changed += OnModelChanged;
            try
            {
                Init();
                if (_disposed) throw new ObjectDisposedException(GetType().Name);
                View.Refresh(Model.CaptureSnapshot());
            }
            catch (Exception initializationError)
            {
                try { Dispose(); }
                catch (Exception cleanupError)
                {
                    throw new AggregateException(initializationError, cleanupError);
                }
                throw;
            }
        }

        protected virtual void Init() { }
        protected virtual void OnDispose() { }

        void OnModelChanged(TSnapshot snapshot)
        {
            if (IsInitialized && !_disposed) View.Refresh(snapshot);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Model.Changed -= OnModelChanged;
            if (!IsInitialized) return;
            IsInitialized = false;
            OnDispose();
        }
    }
}
