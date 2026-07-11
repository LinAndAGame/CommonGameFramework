namespace CardMaster.Framework.StateMachine
{
    /// <summary>
    /// 状态机中的单个状态接口，绑定唯一操作对象类型。
    /// </summary>
    public interface IState<TContent>
    {
        void OnEnter(TContent content);
        void OnExit(TContent content);
        void OnUpdate(TContent content);
    }
}
