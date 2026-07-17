using System.Collections.Generic;

namespace CommonGameFramework.Buff
{
    /// <summary>
    /// 默认 IBuffRegistry 实现；游戏层可替换为 ScriptableObject-backed 版本。
    /// </summary>
    public sealed class BuffRegistry : IBuffRegistry
    {
        readonly Dictionary<string, BaseBuff> _definitions = new Dictionary<string, BaseBuff>();

        public bool Register(BaseBuff definition)
        {
            if (definition == null || string.IsNullOrEmpty(definition.BuffId)) return false;
            _definitions[definition.BuffId] = definition;
            return true;
        }

        public bool TryGetDefinition(string buffId, out BaseBuff definition)
        {
            if (string.IsNullOrEmpty(buffId))
            {
                definition = null;
                return false;
            }

            return _definitions.TryGetValue(buffId, out definition);
        }

        public bool TryCreate(string buffId, object owner, out BuffInstance instance)
        {
            if (!TryGetDefinition(buffId, out var definition) || definition == null)
            {
                instance = null;
                return false;
            }

            instance = definition.CreateInstance(owner);
            return instance != null;
        }
    }
}
