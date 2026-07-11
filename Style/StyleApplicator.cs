using System;
using System.Collections.Generic;
using CardMaster.Framework.StateMachine;

namespace CardMaster.Framework.Style
{
    /// <summary>
    /// 按类型切换样式，内部使用状态机管理当前 Style（可为空）。
    /// </summary>
    public class StyleApplicator<T> where T : class
    {
        readonly StateMachine<T, BaseStyle<T>> _stateMachine;
        readonly Dictionary<string, Type> _styleIdMap = new();

        public T Target => _stateMachine.Content;

        public StyleApplicator(T target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            _stateMachine = new StateMachine<T, BaseStyle<T>>(target);
        }

        public BaseStyle<T> CurrentStyle => _stateMachine.CurrentState;

        public bool HasStyle => _stateMachine.CurrentState != null;

        public void RegisterStyle<TStyle>(TStyle style) where TStyle : BaseStyle<T>
        {
            if (style == null)
                throw new ArgumentNullException(nameof(style));

            _stateMachine.RegisterState(style);

            if (string.IsNullOrEmpty(style.StyleId) == false)
                _styleIdMap[style.StyleId] = typeof(TStyle);
        }

        public void SwitchStyle<TStyle>() where TStyle : BaseStyle<T>
        {
            _stateMachine.ChangeState<TStyle>();
        }

        public void SwitchStyle(string styleId)
        {
            if (string.IsNullOrEmpty(styleId) == true || _styleIdMap.TryGetValue(styleId, out var stateType) == false)
                return;

            _stateMachine.ChangeState(stateType);
        }

        public void ClearStyle()
        {
            _stateMachine.ChangeState(null);
        }
    }
}
