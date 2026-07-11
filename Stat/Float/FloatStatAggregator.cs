using System.Collections.Generic;

namespace CardMaster.Framework.Stat
{
    /// <summary>
    /// 浮点属性聚合器：Flat → PercentAdd → PercentMult，全程 float 运算。
    /// </summary>
    public class FloatStatAggregator : IStatAggregator<float, IStatModifier<float>>
    {
        readonly List<IStatModifier<float>> _modifiers = new List<IStatModifier<float>>();
        float _baseValue;
        bool _dirty = true;
        float _cachedValue;

        public float BaseValue
        {
            get => _baseValue;
            set
            {
                if (System.Math.Abs(_baseValue - value) > float.Epsilon)
                {
                    _baseValue = value;
                    _dirty = true;
                }
            }
        }

        public float Value
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

        public void AddModifier(IStatModifier<float> modifier)
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
            var percentAdd = 0f;
            var percentMult = 1f;

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
                        percentMult *= modifier.Value;
                        break;
                }
            }

            result *= 1f + percentAdd;
            result *= percentMult;
            CachedValue = result;
        }

        protected float CachedValue
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
