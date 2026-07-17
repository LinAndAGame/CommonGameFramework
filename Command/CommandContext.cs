using System;
using System.Collections.Generic;

namespace CommonGameFramework.Command
{
    /// <summary>
    /// 逻辑 Command 上下文；框架仅含 Source/Target 与 services bag，具体服务由游戏层 lazy 注入。
    /// </summary>
    public sealed class CommandContext
    {
        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public object Source { get; set; }
        public object Target { get; set; }

        /// <summary>注册服务实例（同类型覆盖）。</summary>
        public void SetService<T>(T service) where T : class
        {
            if (service == null) return;
            _services[typeof(T)] = service;
        }

        /// <summary>获取已注册服务；未注册返回 null。</summary>
        public T GetService<T>() where T : class
        {
            return TryGetService<T>(out var service) ? service : null;
        }

        /// <summary>尝试获取已注册服务。</summary>
        public bool TryGetService<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj) && obj is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }
    }
}
