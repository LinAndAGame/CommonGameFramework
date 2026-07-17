using System.Collections.Generic;

namespace CommonGameFramework.Achievement
{
    /// <summary>
    /// 多平台 fan-out；游戏层按编译符号 Add Steam / WeGame adapter。
    /// </summary>
    public sealed class CompositeAchievementPlatform : IAchievementPlatform
    {
        readonly List<IAchievementPlatform> _platforms = new List<IAchievementPlatform>();

        public bool IsAvailable
        {
            get
            {
                for (var i = 0; i < _platforms.Count; i++)
                {
                    if (_platforms[i].IsAvailable) return true;
                }

                return false;
            }
        }

        public void Add(IAchievementPlatform platform)
        {
            if (platform == null) return;
            _platforms.Add(platform);
        }

        public void Initialize()
        {
            for (var i = 0; i < _platforms.Count; i++)
                _platforms[i].Initialize();
        }

        public void Unlock(AchievementId id)
        {
            for (var i = 0; i < _platforms.Count; i++)
            {
                if (_platforms[i].IsAvailable)
                    _platforms[i].Unlock(id);
            }
        }

        public void ReportProgress(AchievementId id, int current, int target)
        {
            for (var i = 0; i < _platforms.Count; i++)
            {
                if (_platforms[i].IsAvailable)
                    _platforms[i].ReportProgress(id, current, target);
            }
        }

        public void Flush()
        {
            for (var i = 0; i < _platforms.Count; i++)
                _platforms[i].Flush();
        }
    }
}
