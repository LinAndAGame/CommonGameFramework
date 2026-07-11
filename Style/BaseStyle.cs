using CardMaster.Framework.StateMachine;

namespace CardMaster.Framework.Style
{
    /// <summary>
    /// 样式基类，通过状态机 OnEnter/OnExit 控制样式生命周期。
    /// </summary>
    public abstract class BaseStyle<T> : IState<T> where T : class
    {
        public string StyleId { get; protected set; }

        public virtual void OnEnter(T content) { }
        public virtual void OnExit(T content) { }
        public virtual void OnUpdate(T content) { }
    }
}
