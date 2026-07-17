namespace CommonGameFramework.StateMachine
{
    /// <summary>
    /// 状态机中的单个状态，绑定唯一操作对象类型。
    /// </summary>
    public interface IState<TContent>
    {
        /// <summary>进入该状态时调用。</summary>
        void OnEnter(TContent content);

        /// <summary>离开该状态时调用。</summary>
        void OnExit(TContent content);

        /// <summary>每帧/每拍推进；StyleApplicator 路径不调用，仅直连 StateMachine.Update 时使用。</summary>
        void OnUpdate(TContent content);
    }
}
