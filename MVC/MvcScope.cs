using System;
using System.Collections.Generic;

namespace CommonGameFramework.MVC
{
    /// <summary>实例级装配入口。先注册模型与控制器，再初始化；释放时逆序清理控制器。</summary>
    public sealed class MvcScope : IDisposable
    {
        readonly Dictionary<Type, object> _models = new Dictionary<Type, object>();
        readonly List<IMvcController> _controllers = new List<IMvcController>();
        bool _disposed;
        bool _initializationStarted;

        public bool IsInitialized { get; private set; }

        /// <summary>模型由调用方持有；作用域仅登记引用，不释放外部模型。</summary>
        public void RegisterModel<TModel>(TModel model) where TModel : class
        {
            EnsureCanRegister();
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (_models.ContainsKey(typeof(TModel)))
                throw new InvalidOperationException($"模型类型已注册：{typeof(TModel).FullName}");
            _models.Add(typeof(TModel), model);
        }

        public TModel GetModel<TModel>() where TModel : class
        {
            EnsureAlive();
            if (_models.TryGetValue(typeof(TModel), out var model)) return (TModel)model;
            throw new InvalidOperationException($"模型类型未注册：{typeof(TModel).FullName}");
        }

        /// <summary>控制器生命周期转交给此作用域，不得同时注册到其他作用域。</summary>
        public void RegisterController(IMvcController controller)
        {
            EnsureCanRegister();
            if (controller == null) throw new ArgumentNullException(nameof(controller));
            foreach (var existing in _controllers)
                if (ReferenceEquals(existing, controller))
                    throw new InvalidOperationException("控制器实例已注册。");
            _controllers.Add(controller);
        }

        public void Initialize()
        {
            EnsureAlive();
            if (IsInitialized) return;
            if (_initializationStarted) throw new InvalidOperationException("不能重入初始化。");
            _initializationStarted = true;
            try
            {
                foreach (var controller in _controllers)
                {
                    controller.Initialize();
                    EnsureAlive();
                }
                IsInitialized = true;
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            IsInitialized = false;
            List<Exception> errors = null;
            for (var i = _controllers.Count - 1; i >= 0; i--)
            {
                try { _controllers[i].Dispose(); }
                catch (Exception error)
                {
                    if (errors == null) errors = new List<Exception>();
                    errors.Add(error);
                }
            }
            _controllers.Clear();
            _models.Clear();
            if (errors != null) throw new AggregateException("MVC 控制器释放失败。", errors);
        }

        void EnsureAlive()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MvcScope));
        }

        void EnsureCanRegister()
        {
            EnsureAlive();
            if (_initializationStarted)
                throw new InvalidOperationException("初始化开始后不能修改 MVC 装配。");
        }
    }
}
