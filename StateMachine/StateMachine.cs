using System;
using System.Collections.Generic;

namespace CardMaster.Framework.StateMachine
{
    /// <summary>
    /// 轻量级有限状态机，绑定唯一操作对象，支持注册状态与条件转换。
    /// </summary>
    public class StateMachine<TContent, TState>
        where TContent : class
        where TState : class, IState<TContent>
    {
        readonly Dictionary<Type, TState> _states = new();
        readonly List<IStateTransition<TContent>> _transitions = new();

        /// <summary>状态机唯一操作对象，构造时绑定，生命周期内不变。</summary>
        public TContent Content { get; }

        public TState CurrentState { get; private set; }
        public Type CurrentStateType => CurrentState?.GetType();

        public StateMachine(TContent content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public void RegisterState<T>(T state) where T : class, TState
        {
            _states[typeof(T)] = state;
        }

        public void AddTransition<TFrom, TTo>(Func<TContent, bool> condition = null)
            where TFrom : class, TState
            where TTo : class, TState
        {
            _transitions.Add(new StateTransition<TContent, TFrom, TTo>(condition));
        }

        public void ChangeState<T>() where T : class, TState
        {
            ChangeState(typeof(T));
        }

        public void ChangeState(Type stateType)
        {
            CurrentState?.OnExit(Content);

            if (stateType == null)
            {
                CurrentState = null;
                return;
            }

            if (_states.TryGetValue(stateType, out var nextState) == false)
            {
                throw new InvalidOperationException($"未注册状态: {stateType.Name}");
            }

            CurrentState = nextState;
            CurrentState.OnEnter(Content);
        }

        public void Update()
        {
            CurrentState?.OnUpdate(Content);

            if (CurrentState == null)
            {
                return;
            }

            var currentType = CurrentState.GetType();
            foreach (var transition in _transitions)
            {
                if (transition.MatchesFrom(currentType) == false)
                {
                    continue;
                }

                if (transition.Evaluate(Content) == true)
                {
                    ChangeState(transition.ToStateType);
                    break;
                }
            }
        }
    }
}
