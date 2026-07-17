using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 成就唯一入口：本地存档权威，可选同步到外部平台。
    /// 用法：Register → Load →（可选 AttachPlatform）→ AddProgress / Unlock → Save。
    /// </summary>
    public sealed class AchievementService
    {
        readonly Dictionary<AchievementId, AchievementDefinition> _definitions =
            new Dictionary<AchievementId, AchievementDefinition>();

        readonly IAchievementLocal _local;
        IAchievementPlatform _platform;

        /// <summary>成就解锁时触发（弹窗、音效等）。</summary>
        public event Action<AchievementId> OnUnlocked;

        /// <summary>进度变化时触发（current, target）。</summary>
        public event Action<AchievementId, int, int> OnProgressChanged;

        public AchievementService(IAchievementLocal local)
        {
            _local = local ?? throw new ArgumentNullException(nameof(local));
        }

        /// <summary>
        /// 挂载平台同步 adapter（Steam / WeGame / Composite）。
        /// 传 null 表示仅本地（卸下当前平台，不 Initialize / Sync）。
        /// 非 null 时会 Initialize 并把已有进度回推平台。
        /// </summary>
        public void AttachPlatform(IAchievementPlatform platform)
        {
            _platform = platform;
            if (_platform == null) return;

            _platform.Initialize();
            SyncToPlatform();
        }

        public void Register(AchievementDefinition definition)
        {
            if (definition?.Id == null) return;
            _definitions[definition.Id] = definition;
        }

        public void RegisterRange(IEnumerable<AchievementDefinition> definitions)
        {
            if (definitions == null) return;
            foreach (var def in definitions)
                Register(def);
        }

        public bool TryGetDefinition(AchievementId id, out AchievementDefinition definition)
        {
            if (id != null && _definitions.TryGetValue(id, out definition))
                return true;
            definition = null;
            return false;
        }

        public void Load() => _local.Load();

        public void Save()
        {
            _local.Save();
            _platform?.Flush();
        }

        public bool IsUnlocked(AchievementId id) => id != null && _local.IsUnlocked(id);

        public int GetProgress(AchievementId id) => id == null ? 0 : _local.GetProgress(id);

        /// <summary>累加进度；达到 TargetValue 时自动解锁。须先 Register。</summary>
        public void AddProgress(AchievementId id, int delta)
        {
            if (id == null || delta == 0) return;
            if (_local.IsUnlocked(id)) return;
            if (TryGetRegistered(id, out var def) == false) return;

            SetProgressInternal(id, def, _local.GetProgress(id) + delta);
        }

        /// <summary>设置绝对进度；达到 TargetValue 时自动解锁。须先 Register。</summary>
        public void SetProgress(AchievementId id, int value)
        {
            if (id == null) return;
            if (_local.IsUnlocked(id)) return;
            if (TryGetRegistered(id, out var def) == false) return;

            SetProgressInternal(id, def, value);
        }

        /// <summary>直接解锁（进度型会拉满到 TargetValue）。须先 Register。</summary>
        public void Unlock(AchievementId id)
        {
            if (id == null) return;
            if (TryGetRegistered(id, out var def) == false) return;
            UnlockInternal(id, def);
        }

        bool TryGetRegistered(AchievementId id, out AchievementDefinition def)
        {
            if (_definitions.TryGetValue(id, out def))
                return true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[Achievement] 未 Register 的成就：{id.Key}，已忽略。请先 Register。");
#endif
            def = null;
            return false;
        }

        void SetProgressInternal(AchievementId id, AchievementDefinition def, int value)
        {
            var clamped = value < 0 ? 0 : value;
            if (clamped > def.TargetValue) clamped = def.TargetValue;

            _local.SetProgress(id, clamped);
            OnProgressChanged?.Invoke(id, clamped, def.TargetValue);
            _platform?.ReportProgress(id, clamped, def.TargetValue);

            if (clamped >= def.TargetValue)
                UnlockInternal(id, def);
        }

        void UnlockInternal(AchievementId id, AchievementDefinition def)
        {
            if (_local.IsUnlocked(id)) return;

            _local.MarkUnlocked(id);
            _local.SetProgress(id, def.TargetValue);
            _platform?.Unlock(id);
            OnUnlocked?.Invoke(id);
        }

        void SyncToPlatform()
        {
            if (_platform == null || _platform.IsAvailable == false) return;

            foreach (var pair in _definitions)
            {
                var id = pair.Key;
                var def = pair.Value;
                if (_local.IsUnlocked(id))
                {
                    _platform.Unlock(id);
                    continue;
                }

                var progress = _local.GetProgress(id);
                if (progress > 0)
                    _platform.ReportProgress(id, progress, def.TargetValue);
            }
        }
    }
}
