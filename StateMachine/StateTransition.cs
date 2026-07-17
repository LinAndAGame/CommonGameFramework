using System;

namespace CommonGameFramework.StateMachine
{
    /// <summary>
    /// 供 StateMachine.Update 迭代转换，不暴露裸 Type 构造入口。
    /// </summary>
    public interface IStateTransition<TContent> where TContent : class
    {
        bool MatchesFrom(Type currentStateType);
        Type ToStateType { get; }
        bool Evaluate(TContent content);
    }

    /// <summary>
    /// 描述从一个状态到另一个状态的类型安全转换条件。
    /// </summary>
    public sealed class StateTransition<TContent, TFrom, TTo> : IStateTransition<TContent>
        where TContent : class
        where TFrom : class, IState<TContent>
        where TTo : class, IState<TContent>
    {
        public Func<TContent, bool> Condition { get; }

        public StateTransition(Func<TContent, bool> condition = null)
        {
            Condition = condition ?? (_ => true);
        }

        public bool MatchesFrom(Type currentStateType) => currentStateType == typeof(TFrom);
        public Type ToStateType => typeof(TTo);
        public bool Evaluate(TContent content) => Condition(content);
    }
}
