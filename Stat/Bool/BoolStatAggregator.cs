using System.Collections.Generic;

namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 布尔属性聚合器：任一来源强制 true 则结果为 true，否则使用 BaseValue。
    /// </summary>
    public class BoolStatAggregator : IStatAggregator<bool, IStatModifier<bool>>
    {
        readonly Dictionary<string, bool> _overrides = new Dictionary<string, bool>();
        bool _baseValue;
        bool _dirty = true;
        bool _cachedValue;

        public bool BaseValue
        {
            get => _baseValue;
            set
            {
                if (_baseValue != value)
                {
                    _baseValue = value;
                    _dirty = true;
                }
            }
        }

        public bool Value
        {
            get
            {
                if (_dirty == true)
                {
                    Recalculate();
                }

                return _cachedValue;
            }
        }

        public void AddModifier(IStatModifier<bool> modifier)
        {
            if (modifier == null)
            {
                return;
            }

            _overrides[modifier.SourceId] = modifier.Value;
            _dirty = true;
        }

        public void RemoveModifiersFromSource(string sourceId)
        {
            if (string.IsNullOrEmpty(sourceId) == true)
            {
                return;
            }

            if (_overrides.Remove(sourceId) == true)
            {
                _dirty = true;
            }
        }

        public void ClearModifiers()
        {
            _overrides.Clear();
            _dirty = true;
        }

        protected virtual void Recalculate()
        {
            // 任一来源强制 true → true；否则回落 BaseValue（false 覆盖不压掉 true 基底）
            foreach (var forced in _overrides.Values)
            {
                if (forced)
                {
                    CachedValue = true;
                    return;
                }
            }

            CachedValue = _baseValue;
        }

        protected bool CachedValue
        {
            get => _cachedValue;
            set
            {
                _cachedValue = value;
                _dirty = false;
            }
        }
    }
}
