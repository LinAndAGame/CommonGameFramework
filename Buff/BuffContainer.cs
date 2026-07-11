using System.Collections.Generic;

namespace CardMaster.Framework.Buff
{
    public class BuffContainer
    {
        readonly List<BuffInstance> _buffs = new List<BuffInstance>();

        public IReadOnlyList<BuffInstance> Buffs => _buffs;

        public bool TryAdd(BuffInstance instance)
        {
            if (instance == null) return false;
            var existing = _buffs.Find(b => b.Definition.BuffId == instance.Definition.BuffId);
            if (existing != null)
            {
                switch (instance.Definition.MergePolicy)
                {
                    case BuffMergePolicy.Replace:
                        Remove(existing);
                        break;
                    case BuffMergePolicy.RefreshDuration:
                        existing.Duration = instance.Duration;
                        return true;
                    case BuffMergePolicy.Stack:
                        existing.CurLayer++;
                        return true;
                }
            }
            instance.OnApply();
            _buffs.Add(instance);
            return true;
        }

        public void Remove(BuffInstance instance)
        {
            if (instance == null) return;
            instance.OnRemove();
            _buffs.Remove(instance);
        }

        public void Tick(float deltaTime)
        {
            for (var i = _buffs.Count - 1; i >= 0; i--)
            {
                var buff = _buffs[i];
                buff.Tick(deltaTime);
                if (buff.IsExpired == true) Remove(buff);
            }
        }
    }
}
