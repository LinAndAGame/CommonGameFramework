using System.Collections.Generic;

namespace CommonGameFramework.DragPlace
{
    /// <summary>按 groupId 索引已注册的源和放置点。groupId 为空字符串表示默认组。</summary>
    internal sealed class DragRegistry
    {
        static readonly IReadOnlyList<DragSource> EmptySources = new List<DragSource>();
        static readonly IReadOnlyList<DragPlace> EmptyPlaces = new List<DragPlace>();

        readonly Dictionary<string, List<DragSource>> _sourcesByGroup = new Dictionary<string, List<DragSource>>();
        readonly Dictionary<string, List<DragPlace>> _placesByGroup = new Dictionary<string, List<DragPlace>>();
        readonly Dictionary<DragSource, string> _sourceGroup = new Dictionary<DragSource, string>();
        readonly Dictionary<DragPlace, string> _placeGroup = new Dictionary<DragPlace, string>();
        readonly List<DragSource> _sourceSnapshot = new List<DragSource>();
        readonly List<DragPlace> _placeSnapshot = new List<DragPlace>();

        // 统一空组 ID，避免 null 和空字符串分成两组。
        public static string Normalize(string groupId)
        {
            if (groupId == null)
            {
                return string.Empty;
            }

            return groupId;
        }

        // 登记或改组一个拖拽源。
        public void TrackSource(DragSource source, string groupId)
        {
            if (source == null)
            {
                return;
            }

            UntrackSource(source);
            string key = Normalize(groupId);
            _sourceGroup[source] = key;
            List<DragSource> list = GetOrCreateSources(key);
            list.Add(source);
        }

        // 登记或改组一个放置点。
        public void TrackPlace(DragPlace place, string groupId)
        {
            if (place == null)
            {
                return;
            }

            UntrackPlace(place);
            string key = Normalize(groupId);
            _placeGroup[place] = key;
            List<DragPlace> list = GetOrCreatePlaces(key);
            list.Add(place);
        }

        // 移出拖拽源。
        public void UntrackSource(DragSource source)
        {
            if (source == null)
            {
                return;
            }

            if (_sourceGroup.TryGetValue(source, out string key) == false)
            {
                return;
            }

            _sourceGroup.Remove(source);
            if (_sourcesByGroup.TryGetValue(key, out List<DragSource> list) == true)
            {
                list.Remove(source);
            }
        }

        // 移出放置点。
        public void UntrackPlace(DragPlace place)
        {
            if (place == null)
            {
                return;
            }

            if (_placeGroup.TryGetValue(place, out string key) == false)
            {
                return;
            }

            _placeGroup.Remove(place);
            if (_placesByGroup.TryGetValue(key, out List<DragPlace> list) == true)
            {
                list.Remove(place);
            }
        }

        // 源是否已登记。
        public bool ContainsSource(DragSource source)
        {
            if (source == null)
            {
                return false;
            }

            return _sourceGroup.ContainsKey(source);
        }

        // 放置点是否已登记。
        public bool ContainsPlace(DragPlace place)
        {
            if (place == null)
            {
                return false;
            }

            return _placeGroup.ContainsKey(place);
        }

        // 读取源所在组；未登记则失败。
        public bool TryGetSourceGroup(DragSource source, out string groupId)
        {
            if (source == null)
            {
                groupId = null;
                return false;
            }

            return _sourceGroup.TryGetValue(source, out groupId);
        }

        // 读取放置点所在组；未登记则失败。
        public bool TryGetPlaceGroup(DragPlace place, out string groupId)
        {
            if (place == null)
            {
                groupId = null;
                return false;
            }

            return _placeGroup.TryGetValue(place, out groupId);
        }

        // 该组的源列表。没有成员时返回空列表，不拷贝。
        public IReadOnlyList<DragSource> GetSources(string groupId)
        {
            if (_sourcesByGroup.TryGetValue(Normalize(groupId), out List<DragSource> list) == true)
            {
                return list;
            }

            return EmptySources;
        }

        // 该组的放置点列表。没有成员时返回空列表，不拷贝。
        public IReadOnlyList<DragPlace> GetPlaces(string groupId)
        {
            if (_placesByGroup.TryGetValue(Normalize(groupId), out List<DragPlace> list) == true)
            {
                return list;
            }

            return EmptyPlaces;
        }

        // 拷贝当前全部源，供销毁时解除登记。返回的列表下次调用会被清空。
        public List<DragSource> SnapshotSources()
        {
            _sourceSnapshot.Clear();
            foreach (KeyValuePair<DragSource, string> pair in _sourceGroup)
            {
                _sourceSnapshot.Add(pair.Key);
            }

            return _sourceSnapshot;
        }

        // 拷贝当前全部放置点，供销毁时解除登记。
        public List<DragPlace> SnapshotPlaces()
        {
            _placeSnapshot.Clear();
            foreach (KeyValuePair<DragPlace, string> pair in _placeGroup)
            {
                _placeSnapshot.Add(pair.Key);
            }

            return _placeSnapshot;
        }

        List<DragSource> GetOrCreateSources(string key)
        {
            if (_sourcesByGroup.TryGetValue(key, out List<DragSource> list) == false)
            {
                list = new List<DragSource>();
                _sourcesByGroup.Add(key, list);
            }

            return list;
        }

        List<DragPlace> GetOrCreatePlaces(string key)
        {
            if (_placesByGroup.TryGetValue(key, out List<DragPlace> list) == false)
            {
                list = new List<DragPlace>();
                _placesByGroup.Add(key, list);
            }

            return list;
        }
    }
}
