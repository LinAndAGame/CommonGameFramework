using System.Collections.Generic;

namespace CommonGameFramework.Stat
{
    /// <summary>
    /// 整数属性聚合器：Flat → PercentAdd → PercentMult，全程 int 运算。
    /// PercentAdd 单位为百分点（20 = +20%），PercentMult 单位为百分乘数（200 = 2 倍）。
    /// </summary>
    public class IntStatAggregator : IStatAggregator<int, IStatModifier<int>>
    {
        const int PercentScale = 100;
        const int MultScale = 100;

        readonly List<IStatModifier<int>> _modifiers = new List<IStatModifier<int>>();
        int _baseValue;
        bool _dirty = true;
        int _cachedValue;

        public int BaseValue
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

        public int Value
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

        public void AddModifier(IStatModifier<int> modifier)
        {
            if (modifier == null)
            {
                return;
            }

            _modifiers.Add(modifier);
            _dirty = true;
        }

        public void RemoveModifiersFromSource(string sourceId)
        {
            if (string.IsNullOrEmpty(sourceId) == true)
            {
                return;
            }

            _modifiers.RemoveAll(m => m.SourceId == sourceId);
            _dirty = true;
        }

        public void ClearModifiers()
        {
            _modifiers.Clear();
            _dirty = true;
        }

        protected virtual void Recalculate()
        {
            var result = _baseValue;
            var percentAdd = 0;
            var percentMult = MultScale;

            foreach (var modifier in _modifiers)
            {
                switch (modifier.Type)
                {
                    case ModifierType.Flat:
                        result += modifier.Value;
                        break;
                    case ModifierType.PercentAdd:
                        percentAdd += modifier.Value;
                        break;
                    case ModifierType.PercentMult:
                        percentMult = percentMult * modifier.Value / MultScale;
                        break;
                }
            }

            result = result * (PercentScale + percentAdd) / PercentScale;
            result = result * percentMult / MultScale;
            CachedValue = result;
        }

        protected int CachedValue
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
