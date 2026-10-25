using System;

namespace CommonGameFramework.MVC
{
    /// <summary>控制器生命周期，由所属作用域初始化和释放。</summary>
    public interface IMvcController : IDisposable
    {
        void Initialize();
    }
}
