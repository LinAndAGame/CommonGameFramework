using CommonGameFramework.StateMachine;

namespace CommonGameFramework.Style
{
    /// <summary>
    /// 样式基类；仅用 OnEnter/OnExit 切换外观。StyleApplicator 不调用 OnUpdate / Transition。
    /// </summary>
    public abstract class BaseStyle<T> : IState<T> where T : class
    {
        public string StyleId { get; protected set; }

        public virtual void OnEnter(T content) { }
        public virtual void OnExit(T content) { }

        /// <summary>StyleApplicator 不调用；仅直连 StateMachine.Update 时需要 override。</summary>
        public virtual void OnUpdate(T content) { }
    }
}
